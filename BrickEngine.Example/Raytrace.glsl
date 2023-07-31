#version 460
//#extension GL_ARB_gpu_shader5 : require  
//#extension GL_ARB_bindless_texture : require
const float EPSILON = 1e-8;
const float FloatMax = 3.402823466e+38;
const float MaxDepth = 1000000;
#define localSizeX  8
#define localSizeY  8
#define localSizeZ  1

#define TWO_PI 6.28318530718
#define ONE_OVER_PI (1.0 / PI)
#define ONE_OVER_2PI (1.0 / TWO_PI)
//#define localSize localSizeX * localSizeY * localSizeZ;



struct BVHNode
{
    vec4 bboxA;
    vec4 bboxB;
};

//struct Material
//{
//    uvec2 BaseColorTex;
//    uvec2 MetalRoughnessTex;
//    uvec2 NormalMapTex;
//    uvec2 EmissionTex;
//};

layout (local_size_x = localSizeX, local_size_y = localSizeY) in;

layout(std140, set = 0, binding = 0) uniform CamBuf
{
    mat4 _CameraToWorld;
    mat4 _CameraInverseProjection;
} _CameraProperites;

layout (set = 1, std430, binding = 0) buffer BVHBlockA
{
    BVHNode[] BVHNodes;
}BVHNodeBuffer;

layout (set = 1, std430, binding = 1) buffer BVHBlockB
{
    int[] ModelIds;
}BVHIndexBuffer;

layout (set = 1, std430, binding = 2) buffer BVHBlockC
{
    BVHNode[] BVHNodes;
}BlBVHNodeBuffer;

layout (set = 1, std430, binding = 3) buffer BVHBlockD
{
    int[] BVHIndices;
}BlBVHIndexBuffer;

layout(set = 2, rgba16f, binding = 0) uniform image2D screen;
layout(set = 2, r32f, binding = 1) uniform image2D depth;

struct RayVertexGpu
{
    vec4 PosNx;
    vec4 NormalYZUV;
    vec4 Tangent;
};

struct ModelInfo //Per Model
{
    uint BvhNodesOffset;
    uint BvhPrimIndexOffset;
    uint ModelToMeshesMapIndex;
    uint Padding;
};

layout (set = 3, std430, binding = 0) readonly buffer meshBlockA
{
    uint[] Offsets;
}ObjectIdToModelIdBuffer;

layout (set = 3, std430, binding = 1) readonly buffer meshBlockB
{
    mat4[] Transforms;
}TransformsBuffer;

layout (set = 3, std430, binding = 2) readonly buffer meshBlockC
{
    uint[] MeshIndices;
}ModelIdToMeshesBuffer;

layout (set = 3, std430, binding = 3) readonly buffer meshBlockD
{
    ModelInfo[] ModelInfos;
}ModelInfosBuffer;

layout (set = 3, std430, binding = 4) readonly buffer meshBlockE
{
    RayVertexGpu[] Vertices;
}VerticesBuffer;

layout (set = 3, std430, binding = 5) readonly buffer meshBlockF
{
    uint[] Indices;
}IndicesBuffer;

struct RayHit
{
   int PrimIndex;
   vec3 Position;
   vec3 Normal;
   vec2 BarycentricCoords;
   float Dist;
};

struct Ray
{
    vec3 origin;
    vec3 direction;
    float tmin;
    float tmax;
};

vec3 hsv2rgb(vec3 c)
{
    vec4 K = vec4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
    return c.z * mix(K.xxx, clamp(abs(fract(c.x + K.xyz) * 6.0 - K.w) - K.x, 0.0, 1.0), c.y);
}

Ray CreateRay(vec3 origin, vec3 direction)
{
    Ray ray;
    ray.origin = origin;
    ray.direction = direction;
    ray.tmax = MaxDepth;
    ray.tmin = 0.0;
    return ray;
}

Ray CreateCameraRay(vec2 rayCoords)
{
    // Transform the camera origin to world space
    const vec3 origin = (_CameraProperites._CameraToWorld * vec4(0.0, 0.0, 0.0, 1.0)).xyz;
    
    // Invert the perspective projection of the view-space position
    vec3 direction = (_CameraProperites._CameraInverseProjection * vec4(rayCoords, 0.0, 1.0)).xyz;
    // Transform the direction from camera to world space and normalize
    direction = (_CameraProperites._CameraToWorld * vec4(direction, 0.0)).xyz;
    direction = normalize(direction);
    return CreateRay(origin, direction);
}


float Tonemap_ACES(float x)
{
    // Narkowicz 2015, "ACES Filmic Tone Mapping Curve"
    const float a = 2.51;
    const float b = 0.03;
    const float c = 2.43;
    const float d = 0.59;
    const float e = 0.14;
    return (x * (a * x + b)) / (x * (c * x + d) + e);
}


const float gamma = 2.2;

const float PI = 3.14159265;

vec4 InterpolateVec4(in vec2 barycentricCoords, in vec4 Vector1, in vec4 Vector2, in vec4 Vector3)
{
      return barycentricCoords.x*Vector2+barycentricCoords.y*Vector3+(1-barycentricCoords.x-barycentricCoords.y)*Vector1;
}

vec3 InterpolateVec3(in vec2 barycentricCoords, in vec3 Vector1, in vec3 Vector2, in vec3 Vector3)
{
    return barycentricCoords.x*Vector2+barycentricCoords.y*Vector3+(1-barycentricCoords.x-barycentricCoords.y)*Vector1;
}

vec2 InterpolateVec2(in vec2 barycentricCoords, in vec2 Vector1, in vec2 Vector2, in vec2 Vector3)
{
   return barycentricCoords.x*Vector2+barycentricCoords.y*Vector3+(1-barycentricCoords.x-barycentricCoords.y)*Vector1;
}

float InterpolateFloat(in vec2 barycentricCoords, in float Vector1, in float Vector2, in float Vector3)
{
   return barycentricCoords.x*Vector2+barycentricCoords.y*Vector3+(1-barycentricCoords.x-barycentricCoords.y)*Vector1;
}



vec3 InvertRayDir(in vec3 dir)
{
    return 
    vec3(
    ((abs(dir.x) <= 0.000001) ? (1.0 / dir.x) : (999999999.0 * (dir.x >= 0.0 ? 1.0 : -1.0))),
    ((abs(dir.y) <= 0.000001) ? (1.0 / dir.y) : (999999999.0 * (dir.y >= 0.0 ? 1.0 : -1.0))),
    ((abs(dir.z) <= 0.000001) ? (1.0 / dir.z) : (999999999.0 * (dir.z >= 0.0 ? 1.0 : -1.0)))
    ); 
}


//shared uint stack[gl_WorkGroupSize.x * gl_WorkGroupSize.y][32];

//
//bool TraverseBVHAny(inout Ray ray)
//{
//    uint head = 0;
//    uint wgpidx=gl_LocalInvocationID.x*gl_WorkGroupSize.x+gl_LocalInvocationID.y;
//    stack[wgpidx][head++] = 0;
//    while (head > 0)
//    {
//        BVHNode node = BVHNodeBuffer.BVHNodes[stack[wgpidx][--head]];
//
//        if (!IntersectNode(ray, node.bboxA.xyz, vec3(node.bboxA.w, node.bboxB.x, node.bboxB.y)))
//            continue;
//
//        int PrimCount = floatBitsToInt(node.bboxB.z);
//        int FirstIndex = floatBitsToInt(node.bboxB.w);
//
//        if (PrimCount != 0)
//        {
//            for (uint i = 0; i < PrimCount; ++i)
//            {
//                int prim_index = BVHIndexBuffer.BVHIndices[FirstIndex + i];
//                Vertex v1 = VertexBuffer.Vertices[IndexBuffer.Indices[prim_index * 3 + 0]];
//                Vertex v2 = VertexBuffer.Vertices[IndexBuffer.Indices[prim_index * 3 + 1]];
//                Vertex v3 = VertexBuffer.Vertices[IndexBuffer.Indices[prim_index * 3 + 2]];
//                if(Intersect(v1.Pos, v2.Pos, v3.Pos, ray))
//                {
//                    return true;
//                }
//            }
//        }
//        else
//        {
//            stack[wgpidx][head++] = FirstIndex;
//            stack[wgpidx][head++] = FirstIndex + 1;
//        }
//    }
//    return false;
//}
uvec3 pcg3d(uvec3 v) {
  v = v * 1664525u + 1013904223u;
  v.x += v.y * v.z;
  v.y += v.z * v.x;
  v.z += v.x * v.y;
  v ^= v >> 16u;
  v.x += v.y * v.z;
  v.y += v.z * v.x;
  v.z += v.x * v.y;
  return v;
}


vec3 randomCosineWeightedHemispherePoint(vec3 rand, vec3 n) {
  float r = rand.x * 0.5 + 0.5; // [-1..1) -> [0..1)
  float angle = (rand.y + 1.0) * PI; // [-1..1] -> [0..2*PI)
  float sr = sqrt(r);
  vec2 p = vec2(sr * cos(angle), sr * sin(angle));
  /*
   * Unproject disk point up onto hemisphere:
   * 1.0 == sqrt(x*x + y*y + z*z) -> z = sqrt(1.0 - x*x - y*y)
   */
  vec3 ph = vec3(p.xy, sqrt(1.0 - p*p));
  /*
   * Compute some arbitrary tangent space for orienting
   * our hemisphere 'ph' around the normal. We use the camera's up vector
   * to have some fix reference vector over the whole screen.
   */
  vec3 tangent = normalize(rand);
  vec3 bitangent = cross(tangent, n);
  tangent = cross(bitangent, n);
  
  /* Make our hemisphere orient around the normal. */
  return tangent * ph.x + bitangent * ph.y + n * ph.z;
}


bool IntersectAny(vec3 p0, vec3 p1, vec3 p2, inout Ray ray)
{
    vec3 e1 = p0 - p1;
    vec3 e2 = p2 - p0;
    vec3 n = cross(e1, e2);

    vec3 c = p0 - ray.origin;
    vec3 r = cross(ray.direction, c);
    float inv_det = 1.0 / dot(n, ray.direction);

    float u = dot(r, e2) * inv_det;
    float v = dot(r, e1) * inv_det;


    // These comparisons are designed to return false
    // when one of t, u, or v is a NaN
    if (u >= 0 && v >= 0 &&  (1.0 - u - v) >= 0)
    {
        float t = dot(n, c) * inv_det;
        if (t >= ray.tmin && t <= ray.tmax)
        {
            ray.tmax = t;
            return true;
        }
    }

    return false;
}

bool Intersect(vec3 p0, vec3 p1, vec3 p2, inout Ray ray, inout RayHit hit)
{
    vec3 e1 = p0 - p1;
    vec3 e2 = p2 - p0;
    vec3 n = cross(e1, e2);

    vec3 c = p0 - ray.origin;
    vec3 r = cross(ray.direction, c);
    float inv_det = 1.0 / dot(n, ray.direction);

    float u = dot(r, e2) * inv_det;
    float v = dot(r, e1) * inv_det;
    float w = 1.0 - u - v;

    // These comparisons are designed to return false
    // when one of t, u, or v is a NaN
    if (u >= 0 && v >= 0 && w >= 0)
    {
        float t = dot(n, c) * inv_det;
        if (t >= 0 && t < ray.tmax)
        {
            ray.tmax = t;
            hit.Dist = t;
            hit.Position = ray.origin + t * ray.direction;
            hit.BarycentricCoords = vec2(u, v);
            return true;
        }
    }

    return false;
}

bool IntersectNode(in Ray ray, in vec3 bboxMin, in vec3 bboxMax)
{
    vec3 inv_dir = 1.0 / (ray.direction);
    vec3 tmin_temp = (bboxMin - ray.origin) * inv_dir;
    vec3 tmax_temp = (bboxMax - ray.origin) * inv_dir;
    vec3 tmin = min(tmin_temp, tmax_temp);
    vec3 tmax = max(tmin_temp, tmax_temp);
    
    float hitMin = max(tmin.x, max(tmin.y, max(tmin.z, ray.tmin)));
    float hitMax = min(tmax.x, min(tmax.y, min(tmax.z, ray.tmax)));
    return hitMin <= hitMax;
}
bool rayBoxIntersection(in Ray ray, in vec3 boxMin, in vec3 boxMax) {
    vec3 invRayDir = 1.0 / ray.direction;

    // Calculate t values for the two intersections with the x, y, and z planes of the box
    vec3 t1 = (boxMin - ray.origin) * invRayDir;
    vec3 t2 = (boxMax - ray.origin) * invRayDir;

    // Find the largest minimum t value and the smallest maximum t value
    vec3 tMin = min(t1, t2);
    vec3 tMax = max(t1, t2);

    // Find the largest minimum t value among the x, y, and z components
    float tNear = max(max(tMin.x, tMin.y), tMin.z);

    // Find the smallest maximum t value among the x, y, and z components
    float tFar = min(min(tMax.x, tMax.y), tMax.z);

    // Check if there's a valid intersection
    return tNear <= tFar && tFar >= ray.tmin && tNear <= ray.tmax;
}
//bool IntersectTlNode(in Ray ray, in vec3 bboxMin, in vec3 bboxMax)
//{
//    vec3 invRayDir = 1.0 / ray.direction;
//
//    // Calculate t values for the two intersections with the x, y, and z planes of the box
//    vec3 tMin = (bboxMin - ray.origin) * invRayDir;
//    vec3 tMax = (bboxMax - ray.origin) * invRayDir;
//
//    // Sort the t values to find the nearest and farthest intersection distances
//    vec3 t1 = min(tMin, tMax);
//    vec3 t2 = max(tMin, tMax);
//
//    // Find the largest minimum t value and the smallest maximum t value
//    float tNear = max(max(t1.x, t1.y), t1.z);
//    float tFar = min(min(t2.x, t2.y), t2.z);
//
//    // Check if there's a valid intersection
//    bool intersects = tNear <= tFar && tFar >= 0.0;
//
//    // Update the ray's tmin and tmax values if there is an intersection
//    if (intersects) {
//        ray.tmin = max(ray.tmin, tNear);
//        ray.tmax = min(ray.tmax, tFar);
//    }
//
//    return intersects;
//}
//
//bool IntersectNode(in Ray ray, in vec3 bboxMin, in vec3 bboxMax, out float hitMin, out float hitMax)
//{
//    vec3 inv_dir = 1.0 / (ray.direction);
//    vec3 tmin_temp = (bboxMin - ray.origin) * inv_dir;
//    vec3 tmax_temp = (bboxMax - ray.origin) * inv_dir;
//    vec3 tmin = min(tmin_temp, tmax_temp);
//    vec3 tmax = max(tmin_temp, tmax_temp);
//    
//    hitMin = max(tmin.x, max(tmin.y, max(tmin.z, ray.tmin)));
//    hitMax = min(tmax.x, min(tmax.y, min(tmax.z, ray.tmax)));
//    return hitMin <= hitMax;
//}

shared uint stack[gl_WorkGroupSize.x * gl_WorkGroupSize.y][32];
void TraverseBVH(inout Ray ray, in ModelInfo info, in uint objectId, inout uint head, inout RayHit hit)
{
    uint wgpidx = gl_LocalInvocationID.x*gl_WorkGroupSize.x+gl_LocalInvocationID.y;
    uint start = head;
    stack[wgpidx][head++] = 0;
//    mat4 transform=TransformsBuffer.Transforms[objectId];
//    mat4 t_inv= inverse(transform);
//    ray.origin=(t_inv*vec4(ray.origin, 1)).xyz;
//    ray.direction=(t_inv*vec4(ray.direction, 0)).xyz;
    
    while (head > start)
    {
        BVHNode node = BlBVHNodeBuffer.BVHNodes[info.BvhNodesOffset + stack[wgpidx][--head]];
//        if (!IntersectNode(ray,(vec4(node.bboxA.xyz, 1)*transform).xyz , (vec4(vec3(node.bboxA.w, node.bboxB.x, node.bboxB), 1)*transform).xyz))
        vec3 minAABB = node.bboxA.xyz;
        vec3 maxAABB = vec3(node.bboxA.w, node.bboxB.x, node.bboxB.y);
        if (!IntersectNode(ray, minAABB, maxAABB))
            continue;
           
        uint PrimCount = floatBitsToUint(node.bboxB.z);
        uint FirstIndex = floatBitsToUint(node.bboxB.w);

        if (PrimCount != 0)
        {
            for (uint i = 0; i < PrimCount; ++i)
            {
                
                uint primIndex = BlBVHIndexBuffer.BVHIndices[info.BvhPrimIndexOffset + FirstIndex + i];
                uint meshCount = ModelIdToMeshesBuffer.MeshIndices[info.ModelToMeshesMapIndex];
                
                uint meshIndex = 0;
                uint meshInfoIndex = 0;
                for(; meshIndex<meshCount; meshIndex++)
                {
                    meshInfoIndex = info.ModelToMeshesMapIndex + 1 + meshIndex * 3;
                    uint indexCount = ModelIdToMeshesBuffer.MeshIndices[meshInfoIndex + 1] / 3;
                    if(primIndex-indexCount >= primIndex)
                    {
                        break;
                    }
                    primIndex -= indexCount;
                }
                uint indexOffset = ModelIdToMeshesBuffer.MeshIndices[meshInfoIndex + 0];
                uint vertexOffset = ModelIdToMeshesBuffer.MeshIndices[meshInfoIndex + 2];
                uint primId = indexOffset + primIndex * 3;
                RayVertexGpu v1 = VerticesBuffer.Vertices[vertexOffset + IndicesBuffer.Indices[primId]];
                RayVertexGpu v2 = VerticesBuffer.Vertices[vertexOffset + IndicesBuffer.Indices[primId + 1]];
                RayVertexGpu v3 = VerticesBuffer.Vertices[vertexOffset + IndicesBuffer.Indices[primId + 2]];
                
//                vec3 p1 = (vec4(v1.PosNx.xyz, 1)*transform).xyz;
//                vec3 p2 = (vec4(v2.PosNx.xyz, 1)*transform).xyz;
//                vec3 p3 = (vec4(v3.PosNx.xyz, 1)*transform).xyz;
                vec3 p1 = v1.PosNx.xyz;
                vec3 p2 = v2.PosNx.xyz;
                vec3 p3 = v3.PosNx.xyz;

                if(Intersect(p1, p2, p3, ray, hit))
                {
//                    mat4 matInv = inverse(transform);
//                    hit.Normal = normalize(InterpolateVec3(hit.BarycentricCoords, vec3(matInv * vec4(v1.PosNx.w, v1.NormalYZUV.xy, 0.0)), vec3(matInv * vec4(v2.PosNx.w, v2.NormalYZUV.xy, 0.0)), vec3(matInv * vec4(v3.PosNx.w, v3.NormalYZUV.xy, 0.0))));
                    hit.Normal = normalize(InterpolateVec3(hit.BarycentricCoords, vec3(v1.PosNx.w, v1.NormalYZUV.xy), vec3(v2.PosNx.w, v2.NormalYZUV.xy), vec3(v3.PosNx.w, v3.NormalYZUV.xy)));
                    hit.PrimIndex = int(primId);  
                }
//                    hit.Normal = vec3(v1.PosNx.w, v1.NormalYZUV.xy);
            }
              
        }
        else
        {
            stack[wgpidx][head++] = FirstIndex;
            stack[wgpidx][head++] = FirstIndex + 1;
        }
    }
}

bool TraverseBVHAny(inout Ray ray, in ModelInfo info, in uint objectId, inout uint head)
{
    uint wgpidx = gl_LocalInvocationID.x * gl_WorkGroupSize.x + gl_LocalInvocationID.y;
    uint start = head;
    stack[wgpidx][head++] = 0;
    //    mat4 transform=TransformsBuffer.Transforms[objectId];
    //    mat4 t_inv= inverse(transform);
    //    ray.origin=(t_inv*vec4(ray.origin, 1)).xyz;
    //    ray.direction=(t_inv*vec4(ray.direction, 0)).xyz;

    while (head > start)
    {
        BVHNode node = BlBVHNodeBuffer.BVHNodes[info.BvhNodesOffset + stack[wgpidx][--head]];
        //        if (!IntersectNode(ray,(vec4(node.bboxA.xyz, 1)*transform).xyz , (vec4(vec3(node.bboxA.w, node.bboxB.x, node.bboxB), 1)*transform).xyz))
        vec3 minAABB = node.bboxA.xyz;
        vec3 maxAABB = vec3(node.bboxA.w, node.bboxB.x, node.bboxB.y);
        if (!IntersectNode(ray, minAABB, maxAABB))
            continue;

        uint PrimCount = floatBitsToUint(node.bboxB.z);
        uint FirstIndex = floatBitsToUint(node.bboxB.w);

        if (PrimCount != 0)
        {
            for (uint i = 0; i < PrimCount; ++i)
            {

                uint primIndex = BlBVHIndexBuffer.BVHIndices[info.BvhPrimIndexOffset + FirstIndex + i];
                uint meshCount = ModelIdToMeshesBuffer.MeshIndices[info.ModelToMeshesMapIndex];

                uint meshIndex = 0;
                uint meshInfoIndex = 0;
                for (; meshIndex < meshCount; meshIndex++)
                {
                    meshInfoIndex = info.ModelToMeshesMapIndex + 1 + meshIndex * 3;
                    uint indexCount = ModelIdToMeshesBuffer.MeshIndices[meshInfoIndex + 1] / 3;
                    if (primIndex - indexCount >= primIndex)
                    {
                        break;
                    }
                    primIndex -= indexCount;
                }
                uint indexOffset = ModelIdToMeshesBuffer.MeshIndices[meshInfoIndex + 0];
                uint vertexOffset = ModelIdToMeshesBuffer.MeshIndices[meshInfoIndex + 2];
                uint primId = indexOffset + primIndex * 3;
                RayVertexGpu v1 = VerticesBuffer.Vertices[vertexOffset + IndicesBuffer.Indices[primId]];
                RayVertexGpu v2 = VerticesBuffer.Vertices[vertexOffset + IndicesBuffer.Indices[primId + 1]];
                RayVertexGpu v3 = VerticesBuffer.Vertices[vertexOffset + IndicesBuffer.Indices[primId + 2]];

                //                vec3 p1 = (vec4(v1.PosNx.xyz, 1)*transform).xyz;
                //                vec3 p2 = (vec4(v2.PosNx.xyz, 1)*transform).xyz;
                //                vec3 p3 = (vec4(v3.PosNx.xyz, 1)*transform).xyz;
                vec3 p1 = v1.PosNx.xyz;
                vec3 p2 = v2.PosNx.xyz;
                vec3 p3 = v3.PosNx.xyz;

                if (IntersectAny(p1, p2, p3, ray))
                {
                    return true;
                }
                //                    hit.Normal = vec3(v1.PosNx.w, v1.NormalYZUV.xy);
            }

        }
        else
        {
            stack[wgpidx][head++] = FirstIndex;
            stack[wgpidx][head++] = FirstIndex + 1;
        }
    }
    return false;
}

RayHit TraverseTlBVH(inout Ray ray)
{
    RayHit hit;
    hit.PrimIndex = -1;
    hit.Dist = MaxDepth;
    uint head = 0;
    uint wgpidx = gl_LocalInvocationID.x*gl_WorkGroupSize.x+gl_LocalInvocationID.y;
    stack[wgpidx][head++] = 0;
    while (head > 0)
    {
        BVHNode node = BVHNodeBuffer.BVHNodes[stack[wgpidx][--head]];
        vec3 minAABB = node.bboxA.xyz;
        vec3 maxAABB = vec3(node.bboxA.w, node.bboxB.x, node.bboxB.y);

        if (!IntersectNode(ray, minAABB, maxAABB))
            continue;

        int PrimCount = floatBitsToInt(node.bboxB.z);
        int FirstIndex = floatBitsToInt(node.bboxB.w);
        
        if (PrimCount != 0)
        {
            for (int i = 0; i < PrimCount; ++i)
            {
                uint objectId = BVHIndexBuffer.ModelIds[FirstIndex + i];
                uint modelId = ObjectIdToModelIdBuffer.Offsets[objectId];
                ModelInfo info = ModelInfosBuffer.ModelInfos[modelId];
                TraverseBVH(ray, info, objectId, head, hit);
            }
        }
        else
        {
            stack[wgpidx][head++] = FirstIndex;
            stack[wgpidx][head++] = FirstIndex + 1;
        }
    }
    return hit;
}

bool TraverseTlBVHAny(inout Ray ray)
{
    uint head = 0;
    uint wgpidx = gl_LocalInvocationID.x * gl_WorkGroupSize.x + gl_LocalInvocationID.y;
    stack[wgpidx][head++] = 0;
    while (head > 0)
    {
        BVHNode node = BVHNodeBuffer.BVHNodes[stack[wgpidx][--head]];
        vec3 minAABB = node.bboxA.xyz;
        vec3 maxAABB = vec3(node.bboxA.w, node.bboxB.x, node.bboxB.y);

        if (!IntersectNode(ray, minAABB, maxAABB))
            continue;

        int PrimCount = floatBitsToInt(node.bboxB.z);
        int FirstIndex = floatBitsToInt(node.bboxB.w);

        if (PrimCount != 0)
        {
            for (int i = 0; i < PrimCount; ++i)
            {
                uint objectId = BVHIndexBuffer.ModelIds[FirstIndex + i];
                uint modelId = ObjectIdToModelIdBuffer.Offsets[objectId];
                ModelInfo info = ModelInfosBuffer.ModelInfos[modelId];
                if (TraverseBVHAny(ray, info, objectId, head)) 
                {
                    return true;
                }
            }
        }
        else
        {
            stack[wgpidx][head++] = FirstIndex;
            stack[wgpidx][head++] = FirstIndex + 1;
        }
    }
    return false;
}


const int bounces = 1;
void main() 
{
    const vec3 sunDir = normalize(vec3(2, 1.1, -2));
    ivec2 resolution = imageSize(screen);
    ivec2 pixelPos = ivec2(gl_GlobalInvocationID.xy);
    // Transform pixel to [-1,1] range
    vec2 uv = vec2((pixelPos + vec2(0.5, 0.5)) / vec2(resolution.x, resolution.y));
    vec2 rayCoords = uv * 2.0 - 1.0;
    rayCoords = vec2(rayCoords.x, rayCoords.y);
    Ray ray = CreateCameraRay(rayCoords);
    vec3 color = vec3(0);
    
    for (int bounce = 0; bounce < bounces + 1; ++bounce)
    {
//        float theta = acos(ray.direction.y) / -PI;
//        float phi = atan(ray.direction.x, -ray.direction.z) / -PI * 0.5;
//        vec3 currentColor = texture(_Skybox, vec2(phi, 1 - theta)).rgb;
        vec3 currentColor = vec3(0);
        float strength = max((1 / (bounce * 2.5 + 1)), 0);
        RayHit hit = TraverseTlBVH(ray);
        if(hit.PrimIndex >= 0)
        {
            currentColor = (vec3(1))*(hit.Normal*0.5+0.5);
        }
        else 
        {
            if (dot(ray.direction, sunDir) > 1 - 0.01)
            {
                color = vec3(252 / 255.0, 236 / 255.0, 3 / 255.0) * strength;
            }
            color = vec3(0.1, 0.2, 0.7) * strength;
            break;
        }
//        else 
//        if (hit.PrimIndex < 0)
//        {
//            color = (vec3(hit.PrimIndex));
//        }
//        color = (vec3(info.BvhNodesOffset,0.5,0.5));
//        vec3 faceColor = (hit.Normal * 0.5 + 0.5);
//        currentColor = mix(faceColor, vec3(0,1,1), mix(0, 0.5, clamp(hit.Dist / 60.0, 0.0, 1.0)));
        Ray ShadowRay;
        ShadowRay.origin = hit.Position + hit.Normal * 0.001;
        ShadowRay.direction = sunDir;
        ShadowRay.tmin = 0;
        ShadowRay.tmax = MaxDepth;
        float brightness = clamp(0,1,dot(hit.Normal, sunDir));
        brightness *= TraverseTlBVHAny(ShadowRay) ? 0.5 : 1;
        color += currentColor * brightness * strength;
//        //Setup next bounce
        

        if(bounce < bounces){
            ray.origin = hit.Position + hit.Normal * 0.001;
            ray.direction = normalize(reflect(ray.direction, hit.Normal));
            ray.tmin = 0;
            ray.tmax = MaxDepth;
        }
    }
    
    imageStore(screen, pixelPos, vec4(color, 1.0));    
}
