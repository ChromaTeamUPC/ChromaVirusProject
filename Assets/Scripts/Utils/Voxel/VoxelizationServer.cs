 using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelizationInfo
{
    public List<VoxelizationServer.AABCGrid> aABCGrids;
    public Vector3 voxelScale;
    public bool randomMaterial;
    public Material mat;
    public bool spawnCollider;
    public Vector3 voxelColliderScale;

    public VoxelizationInfo(List<VoxelizationServer.AABCGrid> aABCGrids,
                            Vector3 voxelScale,
                            bool randomMaterial,
                            Material mat,
                            bool spawnCollider,
                            Vector3 voxelColliderScale)
    {
        this.aABCGrids = aABCGrids;
        this.voxelScale = voxelScale;
        this.randomMaterial = randomMaterial;
        this.mat = mat;
        this.spawnCollider = spawnCollider;
        this.voxelColliderScale = voxelColliderScale;
    }
}

public class VoxelizationServer : MonoBehaviour
{
    [SerializeField]
    private int maxVoxelizationGridsPerFrame = 1;
    private Queue<VoxelizationInfo> pendingSpawns = new Queue<VoxelizationInfo>();

    private ColoredObjectsManager colorObjMng;
    private VoxelPool voxelPool;
    private ObjectPool voxelColliderPool;
    private VoxelController voxelController;

    void Start()
    {
        colorObjMng = rsc.coloredObjectsMng;
        voxelPool = rsc.poolMng.voxelPool;
        voxelColliderPool = rsc.poolMng.voxelColliderPool;
    }

    public void AddInfoToPendingSpawns(VoxelizationInfo spawnInfo)
    {
        pendingSpawns.Enqueue(spawnInfo);
    }

    void Update()
    {
        int totalSpawnedThisFrame = 0;

        while(totalSpawnedThisFrame < maxVoxelizationGridsPerFrame && pendingSpawns.Count > 0)
        {
            ++totalSpawnedThisFrame;

            SpawnVoxels(pendingSpawns.Dequeue());
        }

    }

    public void SpawnVoxels(VoxelizationInfo info)
    {
        List<AABCGrid> aABCGrids = info.aABCGrids;

        //int total = 0;
        if (aABCGrids != null)
        {
            foreach (VoxelizationServer.AABCGrid aABCGrid in aABCGrids)
            {
                Vector3 preCalc = aABCGrid.GetOrigin();
                for (short x = 0; x < aABCGrid.GetWidth(); ++x)
                {
                    for (short y = 0; y < aABCGrid.GetHeight(); ++y)
                    {
                        for (short z = 0; z < aABCGrid.GetDepth(); ++z)
                        {
                            if (aABCGrid.IsAABCActiveUnsafe(x, y, z))
                            {
                                Vector3 cubeCenter = aABCGrid.GetAABCCenterUnsafe(x, y, z) + preCalc;

                                voxelController = voxelPool.GetObject();
                                if (voxelController != null)
                                {
                                    //++total;
                                    Transform voxelTrans = voxelController.gameObject.transform;
                                    voxelTrans.position = cubeCenter;
                                    voxelTrans.rotation = Quaternion.identity;
                                    //voxelTrans.rotation = Random.rotation;
                                    voxelTrans.localScale = info.voxelScale;
                                    if (!info.randomMaterial)
                                    {
                                        voxelController.GetComponent<Renderer>().sharedMaterial = info.mat;
                                    }
                                    else
                                    {
                                        voxelController.GetComponent<Renderer>().sharedMaterial = colorObjMng.GetVoxelRandomMaterial();
                                    }

                                    voxelController.spawnLevels = 1;
                                }
                            }
                        }
                    }
                }

                //Set a collider in place to make voxels "explode"
                if (info.spawnCollider)
                {
                    GameObject voxelCollider = voxelColliderPool.GetObject();
                    if (voxelCollider != null)
                    {
                        voxelCollider.transform.localScale = info.voxelColliderScale;
                        voxelCollider.transform.position = aABCGrid.GetCenter();
                    }
                }
            }
        }
        //Debug.Log("Spider spawned: " + total);
    }

    //New API (Faster but some preprocess needed)
    public static AABCGrid Create1Grid1Object(Transform transform, Mesh mesh, Renderer renderer, float cubeSide, bool includeInside)
    {
        AABCGrid grid = new AABCGrid(renderer.bounds.min, renderer.bounds.max, cubeSide);
        grid.FillGridWithGameObjectInfo(transform, mesh.vertices, mesh.triangles, includeInside);
        return grid;
    }

    public static AABCGrid Create1GridNObjects(List<Transform> transforms, List<Mesh> meshes, List<Renderer> renderers, float cubeSide, bool includeInside)
    {
        if (transforms.Count == 0) return null;

        Vector3 gridBoundsMin = renderers[0].bounds.min;
        Vector3 gridBoundsMax = renderers[0].bounds.max;

        for (int i = 1; i < renderers.Count; ++i)
        {
            gridBoundsMin = Vector3.Min(gridBoundsMin, renderers[i].bounds.min);
            gridBoundsMax = Vector3.Max(gridBoundsMax, renderers[i].bounds.max);
        }

        AABCGrid grid = new AABCGrid(gridBoundsMin, gridBoundsMax, cubeSide);

        for (int i = 0; i < transforms.Count; ++i)
        {
            grid.FillGridWithGameObjectInfo(transforms[i], meshes[i].vertices, meshes[i].triangles, includeInside);
        }

        return grid;
    }

    public static List<AABCGrid> CreateNGridsNObjects(List<Transform> transforms, List<Mesh> meshes, List<Renderer> renderers, float cubeSide, bool includeInside)
    {
        List<AABCGrid> res = new List<AABCGrid>();

        for (int i = 0; i < transforms.Count; ++i)
        {
            res.Add(Create1Grid1Object(transforms[i], meshes[i], renderers[i], cubeSide, includeInside));
        }

        return res;
    }

    //Old API (Simpler to call, but it's slower because it has to do some searches)
    public static AABCGrid CreateGridWithGameObjectMesh(GameObject gameObj, float cubeSide, bool includeChildren, bool includeInside)
    {
        if (!includeChildren)
            return CreateGridWithSingleObject(gameObj, cubeSide, includeInside);
        else
            return CreateGridWithMultipleObjects(gameObj, cubeSide, includeInside);
    }

    public static List<AABCGrid> CreateMultipleGridsWithGameObjectMesh(GameObject gameObj, float cubeSide, bool includeInside)
    {
        List<AABCGrid> res = new List<AABCGrid>();

        CreateMultipleGridsWithGameObjectMeshRec(gameObj, res, cubeSide, includeInside);

        return res;
    }

    //Old API private functions
    private static void CreateMultipleGridsWithGameObjectMeshRec(GameObject gameObj, List<AABCGrid> list, float cubeSide, bool includeInside)
    {
        AABCGrid res = CreateGridWithSingleObject(gameObj, cubeSide, includeInside);
        if (res != null)
            list.Add(res);

        //Call children
        for (int i = 0; i < gameObj.transform.childCount; ++i)
        {
            CreateMultipleGridsWithGameObjectMeshRec(gameObj.transform.GetChild(i).gameObject, list, cubeSide, includeInside);
        }
    }

    private static AABCGrid CreateGridWithSingleObject(GameObject gameObj, float cubeSide, bool includeInside)
    {
        //Ext Opt 1: Get objects once
        Renderer rend = gameObj.GetComponent<Renderer>();
        if (rend != null)
        {
            Mesh gameObjMesh = gameObj.GetComponent<MeshFilter>().mesh;

            //Ext Opt 2: Create AABCGrid without temp objects
            AABCGrid grid = new AABCGrid(rend.bounds.min, rend.bounds.max, cubeSide);
            grid.FillGridWithGameObjectInfo(gameObj.transform, gameObjMesh.vertices, gameObjMesh.triangles, includeInside);
            return grid;
        }
        else
        {
            SkinnedMeshRenderer sRend = gameObj.GetComponent<SkinnedMeshRenderer>();
            if (sRend != null)
            {
                Mesh gameObjMesh = new Mesh();
                sRend.BakeMesh(gameObjMesh);

                //Ext Opt 2: Create AABCGrid without temp objects
                AABCGrid grid = new AABCGrid(sRend.bounds.min, sRend.bounds.max, cubeSide);
                grid.FillGridWithGameObjectInfo(gameObj.transform, gameObjMesh.vertices, gameObjMesh.triangles, includeInside);
                return grid;

            }
            else
                return null;
        }
    }

    private static AABCGrid CreateGridWithMultipleObjects(GameObject gameObj, float cubeSide, bool includeInside)
    {
        List<GameObject> gameObjectsWithMesh = new List<GameObject>();
        List<Bounds> gameObjectBounds = new List<Bounds>();

        GetObjectsWithMeshRec(gameObj, gameObjectsWithMesh, gameObjectBounds);

        if (gameObjectsWithMesh.Count == 0) return null;

        Vector3 gridBoundsMin = gameObjectBounds[0].min;
        Vector3 gridBoundsMax = gameObjectBounds[0].max;

        for(int i = 1; i < gameObjectBounds.Count; ++i)
        {
            gridBoundsMin = Vector3.Min(gridBoundsMin, gameObjectBounds[i].min);
            gridBoundsMax = Vector3.Max(gridBoundsMax, gameObjectBounds[i].max);
        }

        //Ext Opt 2: Create AABCGrid without temp objects
        AABCGrid grid = new AABCGrid(gridBoundsMin, gridBoundsMax, cubeSide);

        for (int i = 0; i < gameObjectsWithMesh.Count; ++i)
        {
            GameObject gameObjAux = gameObjectsWithMesh[i];
            Mesh gameObjMesh = null;
            if (gameObjAux.GetComponent<MeshFilter>() != null)
                gameObjMesh = gameObjAux.GetComponent<MeshFilter>().mesh;
            else if (gameObjAux.GetComponent<SkinnedMeshRenderer>() != null)
            {
                gameObjMesh = new Mesh();
                gameObjAux.GetComponent<SkinnedMeshRenderer>().BakeMesh(gameObjMesh);
            }

            grid.FillGridWithGameObjectInfo(gameObjAux.transform, gameObjMesh.vertices, gameObjMesh.triangles, includeInside);
        }

        return grid;
    } 

    private static void GetObjectsWithMeshRec(GameObject gameObj, List<GameObject> list, List<Bounds> bounds)
    {
        //Add game object
        //Ext Opt 1: Get objects once
        Renderer rend = gameObj.GetComponent<Renderer>();
        if (rend != null)
        {
            list.Add(gameObj);
            bounds.Add(rend.bounds);
        }
        else
        {
            SkinnedMeshRenderer sRend = gameObj.GetComponent<SkinnedMeshRenderer>();
            if (sRend != null)
            {
                list.Add(gameObj);
                bounds.Add(sRend.bounds);
            }
        }

        //Call children
        for (int i = 0; i < gameObj.transform.childCount; ++i)
        {
            GetObjectsWithMeshRec(gameObj.transform.GetChild(i).gameObject, list, bounds);
        }
    } 
   

    //AABC stands for Axis Aligned Bounding Cube
    public class AABCGrid
    {
        private static bool debug = false;

        public short width;
        public short height;
        public short depth;

        private float side;
        private float halfSide; //Optimization: halfSide precalculated
        
        private Vector3 origin;
        private Vector3 centerOffset; //Optimization 3: Center precalculated
        private Vector3 center; //Optimization 3: Center precalculated
        private bool[,,] cubeActive;
        private short[,,] cubeNormalSum;

        //Optimization 4: Objects precreated. Used in triangle intersect AABC function
        private Vector3[] aabcVertices = { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0) };
        //Optimization 15: Triangle vertices not copied between function calls
        private Vector3[] triangleVertices = new Vector3[3];

        //Optimization 14: Axes static
        private static Vector3 axisX = new Vector3(1, 0, 0);
        private static Vector3 axisY = new Vector3(0, 1, 0);
        private static Vector3 axisZ = new Vector3(0, 0, 1);

        private Vector3 triangleEdgeA, triangleEdgeB, triangleEdgeC;
        private Vector3 triangleNormal;

        //Optimization 8: Predeclared vectors to calculate axes to check intersection
        private Vector3 axisAX;
        private Vector3 axisAY;
        private Vector3 axisAZ;
        private Vector3 axisBX;
        private Vector3 axisBY;
        private Vector3 axisBZ;
        private Vector3 axisCX;
        private Vector3 axisCY;
        private Vector3 axisCZ;

        //start defining class AABCGrid

        /*-----------------------------------------------------------------------------------------------------------------------------
        ---- CONSTRUCTORS -------------------------------------------------------------------------------------------------------------
        -----------------------------------------------------------------------------------------------------------------------------*/
        public AABCGrid(short x, short y, short z, float sideLength, Vector3 cent)
        {
            width = x;
            height = y;
            depth = z;
            side = sideLength;
            halfSide = side / 2;
            center = cent;
            //Optimization 3: Origin precalculated
            centerOffset = new Vector3(width / 2 * side, height / 2 * side, depth / 2 * side);
            origin = center - centerOffset;

            cubeActive = new bool[width, height, depth];
        }

        public  AABCGrid(Vector3 gridBoundsMin, Vector3 gridBoundsMax, float cubeSide)
        {
            width = (short)(Mathf.Ceil((gridBoundsMax.x - gridBoundsMin.x) / cubeSide));
            height = (short)(Mathf.Ceil((gridBoundsMax.y - gridBoundsMin.y) / cubeSide));
            depth = (short)(Mathf.Ceil((gridBoundsMax.z - gridBoundsMin.z) / cubeSide));
            side = cubeSide;
            halfSide = side / 2;

            origin.x = gridBoundsMin.x - (((width * cubeSide) - (gridBoundsMax.x - gridBoundsMin.x)) / 2);
            origin.y = gridBoundsMin.y - (((height * cubeSide) - (gridBoundsMax.y - gridBoundsMin.y)) / 2);
            origin.z = gridBoundsMin.z - (((depth * cubeSide) - (gridBoundsMax.z - gridBoundsMin.z)) / 2);

            //Optimization 3: Center precalculated
            centerOffset = new Vector3(width / 2 * side, height / 2 * side, depth / 2 * side);
            center = origin + centerOffset;

            cubeActive = new bool[width, height, depth];
        }
        

        /*-----------------------------------------------------------------------------------------------------------------------------
        ---- BASIC GETTERS AND SETTERS ------------------------------------------------------------------------------------------------
        -----------------------------------------------------------------------------------------------------------------------------*/
        public void SetDebug(bool debug)
        {
            AABCGrid.debug = debug;
        }

        public short GetWidth()
        {
            return width;
        }

        public short GetHeight()
        {
            return height;
        }

        public short GetDepth()
        {
            return depth;
        }

        public float GetCubeSide()
        {
            return side;
        }

        public Vector3 GetOrigin()
        {
            return origin;
        }

        public Vector3 GetCenter()
        {
            return center;
        }

        public void SetCenter(Vector3 cent)
        {
            center = cent;
            //Optimization 3: Origin precalculated
            origin = center - centerOffset;
        }

        public int GetTotalAABCCount()
        {
            return width * height * depth;
        }

        public int GetActiveAABCCount()
        {
            int count = 0;

            for (short x = 0; x < width; ++x)
            {
                for (short y = 0; y < height; ++y)
                {
                    for (short z = 0; z < depth; z++)
                    {
                        if (IsAABCActiveUnsafe(x, y, z)) 
                        {
                            ++count;
                        }
                    }
                }
            }

            return count;
        }


        /*-----------------------------------------------------------------------------------------------------------------------------
        ---- ADVANCED GETTERS ---------------------------------------------------------------------------------------------------------
        -----------------------------------------------------------------------------------------------------------------------------*/

        public void CheckIndexes(short x, short y, short z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= width || y >= height || z >= depth)
            {
                throw new System.ArgumentOutOfRangeException("The requested AABC is out of the grid limits.");
            }
        }

        public Vector3 GetAABCCenter(short x, short y, short z)
        {
            CheckIndexes(x, y, z);
            return GetAABCCenterUnsafe(x, y, z);
        }

        public Vector3 GetAABCCenterUnsafe(short x, short y, short z)
        {
            return new Vector3(x * side + halfSide, y * side + halfSide, z * side + halfSide);
        }

        public bool IsAABCActive(short x, short y, short z)
        {
            CheckIndexes(x, y, z);
            return IsAABCActiveUnsafe(x, y, z);
        }

        public bool IsAABCActiveUnsafe(short x, short y, short z)
        {
            return cubeActive[x, y, z];
        }

        /*-----------------------------------------------------------------------------------------------------------------------------
        ---- CORE FUNCTIONALITY -------------------------------------------------------------------------------------------------------
        -----------------------------------------------------------------------------------------------------------------------------*/
        
        public void FillGridWithFixedNumber(int count)
        {
            int total = 0;

            int x, y, z;
            x = 0;
            while (x < width && total < count)
            {
                y = 0;
                while (y < height && total < count)
                {
                    z = 0;
                    while (z < depth && total < count)
                    {
                        cubeActive[x, y, z] = true;
                        ++total;

                        ++z;
                    }

                    ++y;
                }

                ++x;
            }
        }   

        public void FillGridWithGameObjectInfo(Transform gameObjTransf, Vector3[] meshVertices, int[] meshTriangles, bool fillInside)
        {
            float startTime = 0.0f; float endTime;

            if (debug)
            {
                Debug.Log("--Filling grid");
                Debug.Log("--Mesh Triangles: " + meshTriangles.Length / 3);
                Debug.Log("--Grid Size: " + width + ',' + height + ',' + depth);
                startTime = Time.realtimeSinceStartup;
            }

            int meshTrianglesCount = meshTriangles.Length / 3;
            Vector3 triangleMin, triangleMax;
            short startX, startY, startZ;
            short endX, endY, endZ;

            short x, y, z;     

            if (fillInside)
                cubeNormalSum = new short[width, height, depth];         

            //Optimization 2: Calculate only once the TransformPoint for each vertice.
            for (int i = 0; i < meshVertices.Length; ++i)
            {
                //TransformPoint translates from local space to world space
                meshVertices[i] = gameObjTransf.TransformPoint(meshVertices[i]);
            }

            for (int i = 0; i < meshTrianglesCount; ++i)
            {
                triangleVertices[0] = meshVertices[meshTriangles[i * 3]];
                triangleVertices[1] = meshVertices[meshTriangles[i * 3 + 1]];
                triangleVertices[2] = meshVertices[meshTriangles[i * 3 + 2]];

                //Optimization 7: Triangle edges and normal calculated only once per triangle
                triangleEdgeA = triangleVertices[1] - triangleVertices[0];
                triangleEdgeB = triangleVertices[2] - triangleVertices[1];
                triangleEdgeC = triangleVertices[0] - triangleVertices[2];
                triangleNormal = Vector3.Cross(triangleEdgeA, triangleEdgeB).normalized;

                //Optimization 8: Calculate axes only once per triangle
                axisAX = Vector3.Cross(triangleEdgeA, axisX);
                axisBX = Vector3.Cross(triangleEdgeB, axisX);
                axisCX = Vector3.Cross(triangleEdgeC, axisX);
                axisAY = Vector3.Cross(triangleEdgeA, axisY);
                axisBY = Vector3.Cross(triangleEdgeB, axisY);
                axisCY = Vector3.Cross(triangleEdgeC, axisY);
                axisAZ = Vector3.Cross(triangleEdgeA, axisZ);
                axisBZ = Vector3.Cross(triangleEdgeB, axisZ);
                axisCZ = Vector3.Cross(triangleEdgeC, axisZ);

                //Optimization 0. Assign x, y and z only once to aux, and search for start and end together. Cast to short once
                triangleMin = (Vector3.Min(Vector3.Min(triangleVertices[0], triangleVertices[1]), triangleVertices[2]) - origin) / side;
                triangleMax = (Vector3.Max(Vector3.Max(triangleVertices[0], triangleVertices[1]), triangleVertices[2]) - origin) / side;

                startX = (short)(Mathf.Floor(triangleMin.x));
                endX = (short)(Mathf.Ceil(triangleMax.x));
                startY = (short)(Mathf.Floor(triangleMin.y));
                endY = (short)(Mathf.Ceil(triangleMax.y));
                startZ = (short)(Mathf.Floor(triangleMin.z));
                endZ = (short)(Mathf.Ceil(triangleMax.z));

                if (startX < 0) startX = 0;
                if (endX > width) endX = width;
                if (startY < 0) startY = 0;
                if (endY > height) endY = height;
                if (startZ < 0) startZ = 0;
                if (endZ > depth) endZ = depth;

                if (fillInside)
                {
                    for (x = startX; x < endX; ++x)
                    {
                        for (y = startY; y < endY; ++y)
                        {
                            for (z = startZ; z < endZ; ++z)
                            {
                                if (CheckTriangleAABCIntersection(x, y, z))
                                {
                                    cubeActive[x, y, z] = true;

                                    if (triangleNormal.z < 0)
                                    {
                                        ++cubeNormalSum[x, y, z];
                                    }
                                    else if (triangleNormal.z > 0)
                                    {
                                        --cubeNormalSum[x, y, z];
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (x = startX; x < endX; ++x)
                    {
                        for (y = startY; y < endY; ++y)
                        {
                            for (z = startZ; z < endZ; ++z)
                            {
                                if (!IsAABCActiveUnsafe(x, y, z) && CheckTriangleAABCIntersection(x, y, z))
                                {
                                    cubeActive[x, y, z] = true;
                                }
                            }
                        }
                    }
                }
            }

            if (fillInside)
            {

                for (x = 0; x < width; ++x)
                {
                    for (y = 0; y < height; ++y)
                    {
                        bool fill = false;
                        int cubeToFill = 0;

                        for (z = 0; z < depth; ++z)
                        {
                            if (cubeActive[x, y, z])
                            {
                                short normalSum = cubeNormalSum[x, y, z];
                                if (normalSum != 0)
                                {
                                    if (normalSum > 0)
                                    {
                                        fill = true;
                                    }
                                    else
                                    {
                                        fill = false;
                                        while (cubeToFill >= 1)
                                        {
                                            cubeActive[x, y, z - cubeToFill] = true;
                                            --cubeToFill;
                                        }
                                    }
                                    cubeToFill = 0;
                                }
                                continue;
                            }
                            if (fill)
                            {
                                ++cubeToFill;
                            }
                        }
                    }
                }
                cubeNormalSum = null;
            }           

            
            if (debug)
            {
                endTime = Time.realtimeSinceStartup;          
                Debug.Log("--TIME SPENT: " + (endTime - startTime) + "s");
                Debug.Log("--Total grid cubes: " + GetTotalAABCCount());
                Debug.Log("--Total active cubes: " + GetActiveAABCCount());
            }
        }

        private bool CheckTriangleAABCIntersection(short x, short y, short z)
        {
            
            CalculateAABCVertices(x, y, z);

            //Optimization 7: Calculate trianglesEdge only once per triangle (code moved to FillGridWithGameObjectMeshInside)
            //Optimization 8: Calculate axes only once per triangle (code moved to FillGridWithGameObjectMeshInside)
            //Optimization 9: First check for triangleNormal axis (it discards more intersections!)

            if (!ProjectionsIntersectOnAxis(triangleNormal)) return false;

            if (!ProjectionsIntersectOnAxis(axisAX)) return false;
            if (!ProjectionsIntersectOnAxis(axisAY)) return false;
            if (!ProjectionsIntersectOnAxis(axisAZ)) return false;

            if (!ProjectionsIntersectOnAxis(axisBX)) return false;
            if (!ProjectionsIntersectOnAxis(axisBY)) return false;
            if (!ProjectionsIntersectOnAxis(axisBZ)) return false;

            if (!ProjectionsIntersectOnAxis(axisCX)) return false;
            if (!ProjectionsIntersectOnAxis(axisCY)) return false;
            if (!ProjectionsIntersectOnAxis(axisCZ)) return false;

            //if (!ProjectionsIntersectOnAxis(axisX)) return false;
            //if (!ProjectionsIntersectOnAxis(axisY)) return false;
            //if (!ProjectionsIntersectOnAxis(axisZ)) return false;

            return true;
        }

        private void CalculateAABCVertices(short x, short y, short z)
        {
            //Optimization 6: aabcVertices precreated. Method only changes values
            //Opt 12: No need new aux object

            //Precalc
            float xMinus = origin.x + (x * side);
            float xPlus = xMinus + side;
            float yMinus = origin.y + (y * side);
            float yPlus = yMinus + side;
            float zMinus = origin.z + (z * side);
            float zPlus = zMinus + side;       

            aabcVertices[0].x = xPlus; aabcVertices[0].y = yMinus; aabcVertices[0].z = zPlus;
            aabcVertices[1].x = xPlus; aabcVertices[1].y = yMinus; aabcVertices[1].z = zMinus;
            aabcVertices[2].x = xMinus; aabcVertices[2].y = yMinus; aabcVertices[2].z = zPlus;
            aabcVertices[3].x = xMinus; aabcVertices[3].y = yMinus; aabcVertices[3].z = zMinus;
            aabcVertices[4].x = xPlus; aabcVertices[4].y = yPlus; aabcVertices[4].z = zPlus;
            aabcVertices[5].x = xPlus; aabcVertices[5].y = yPlus; aabcVertices[5].z = zMinus;
            aabcVertices[6].x = xMinus; aabcVertices[6].y = yPlus; aabcVertices[6].z = zPlus;
            aabcVertices[7].x = xMinus; aabcVertices[7].y = yPlus; aabcVertices[7].z = zMinus;
        }

        private bool ProjectionsIntersectOnAxis(Vector3 axis)
        {
            //Optimization 1: Get max and min with single method call
            //Optimization 11: Unravel the loop
            //Optimization 13: Avoid all calculations searching for min and max
            //Optimization 15: Avoid all calculations on triangle too

            //Triangle projection
            //Triangle projection Vertex 0
            float dotProd = Vector3.Dot(triangleVertices[0], axis);
            float minTriangle = dotProd;
            float maxTriangle = dotProd;

            //Triangle projection Vertex 1
            dotProd = Vector3.Dot(triangleVertices[1], axis);
            if (dotProd > maxTriangle) maxTriangle = dotProd;
            if (dotProd < minTriangle) minTriangle = dotProd;

            //AABC projection
            //AABC projection Vertex 0
            dotProd = Vector3.Dot(aabcVertices[0], axis);
            float minAABC = dotProd;
            float maxAABC = dotProd;
            if (!((minTriangle > maxAABC) || (maxTriangle < minAABC))) return true;

            //Triangle projection Vertex 2
            dotProd = Vector3.Dot(triangleVertices[2], axis);
            if (dotProd > maxTriangle) maxTriangle = dotProd;
            if (dotProd < minTriangle) minTriangle = dotProd;
            if (!((minTriangle > maxAABC) || (maxTriangle < minAABC))) return true;

            //AABC projection Vertex 1
            dotProd = Vector3.Dot(aabcVertices[1], axis);
            if (dotProd > maxAABC) maxAABC = dotProd;
            if (dotProd < minAABC) minAABC = dotProd;
            if (!((minTriangle > maxAABC) || (maxTriangle < minAABC))) return true;

            //AABC projection Vertex 2
            dotProd = Vector3.Dot(aabcVertices[2], axis);
            if (dotProd > maxAABC) maxAABC = dotProd;
            if (dotProd < minAABC) minAABC = dotProd;
            if (!((minTriangle > maxAABC) || (maxTriangle < minAABC))) return true;

            //AABC projection Vertex 3
            dotProd = Vector3.Dot(aabcVertices[3], axis);
            if (dotProd > maxAABC) maxAABC = dotProd;
            if (dotProd < minAABC) minAABC = dotProd;
            if (!((minTriangle > maxAABC) || (maxTriangle < minAABC))) return true;

            //AABC projection Vertex 4
            dotProd = Vector3.Dot(aabcVertices[4], axis);
            if (dotProd > maxAABC) maxAABC = dotProd;
            if (dotProd < minAABC) minAABC = dotProd;
            if (!((minTriangle > maxAABC) || (maxTriangle < minAABC))) return true;

            //AABC projection Vertex 5
            dotProd = Vector3.Dot(aabcVertices[5], axis);
            if (dotProd > maxAABC) maxAABC = dotProd;
            if (dotProd < minAABC) minAABC = dotProd;
            if (!((minTriangle > maxAABC) || (maxTriangle < minAABC))) return true;

            //AABC projection Vertex 6
            dotProd = Vector3.Dot(aabcVertices[6], axis);
            if (dotProd > maxAABC) maxAABC = dotProd;
            if (dotProd < minAABC) minAABC = dotProd;
            if (!((minTriangle > maxAABC) || (maxTriangle < minAABC))) return true;

            //AABC projection Vertex 7
            dotProd = Vector3.Dot(aabcVertices[7], axis);
            if (dotProd > maxAABC) maxAABC = dotProd;
            if (dotProd < minAABC) minAABC = dotProd;
            if (!((minTriangle > maxAABC) || (maxTriangle < minAABC))) return true;

            return false;          
        }
    }
    //end class AABCGrid
}

