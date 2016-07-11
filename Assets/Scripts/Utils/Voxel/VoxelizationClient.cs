using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelizationClient : MonoBehaviour {

    //Behaviour variables
    public float voxelSideSize = 0.2f;
    public bool fillVoxelShell = false;

    public bool includeChildren = true;
    public bool createMultipleGrids = false;

    public bool randomMaterial = false;

    public bool spawnVoxelCollider = true;
    public float voxelColliderScaleRatio = 1f;
    public int fakeGridMinVoxels;
    public int fakeGridMaxVoxels;

    private List<VoxelizationServer.AABCGrid> aABCGrids;
    private VoxelizationServer.AABCGrid fakeAABCGrid;
    private List<VoxelizationServer.AABCGrid> fakeAABCGrids = new List<VoxelizationServer.AABCGrid>();

    private ColoredObjectsManager colorObjMng;
    private VoxelPool voxelPool;
    private ObjectPool voxelColliderPool;
    private VoxelController voxelController;

    private bool spawnColliderThisTime;

    //Precalculated variables (so calculation of grid will be faster on realtime)
    Material mat;
    Transform transf;
    Renderer rend;
    SkinnedMeshRenderer sRend;
    MeshFilter meshFilter;
    List<Transform> transforms;
    List<Renderer> renderers;
    List<bool> isSkinedMeshRenderer;
    List<MeshFilter> meshFilters;
    List<Mesh> meshes;

    Vector3 voxelScale;
    Vector3 voxelColliderScale;

    // Use this for initialization
    void Start ()
    {
        colorObjMng = rsc.coloredObjectsMng;
        voxelPool = rsc.poolMng.voxelPool;
        voxelColliderPool = rsc.poolMng.voxelColliderPool;

        mat = colorObjMng.GetVoxelRandomMaterial();

        if (!includeChildren)
        {
            transf = gameObject.transform;
            rend = gameObject.GetComponent<Renderer>();
            meshFilter = gameObject.GetComponent<MeshFilter>();
            sRend = gameObject.GetComponent<SkinnedMeshRenderer>();
        }
        else
        {
            transforms = new List<Transform>();
            renderers = new List<Renderer>();
            isSkinedMeshRenderer = new List<bool>();
            meshFilters = new List<MeshFilter>();
            meshes = new List<Mesh>();

            PopulateLists(gameObject);
        }

        voxelScale = new Vector3(voxelSideSize, voxelSideSize, voxelSideSize);
        voxelColliderScale = new Vector3(voxelColliderScaleRatio, voxelColliderScaleRatio, voxelColliderScaleRatio);
        spawnColliderThisTime = spawnVoxelCollider;

        CalculateFakeVoxelsGrid();
    }

    private void PopulateLists(GameObject gameObj)
    {
        SkinnedMeshRenderer sRend = gameObj.GetComponent<SkinnedMeshRenderer>();
        if (sRend != null)
        {
            transforms.Add(gameObj.transform);
            renderers.Add(sRend);
            meshFilters.Add(null);
            isSkinedMeshRenderer.Add(true);
        }
        else
        {
            Renderer rend = gameObj.GetComponent<Renderer>();
            if (rend != null)
            {
                transforms.Add(gameObj.transform);
                renderers.Add(rend);
                meshFilters.Add(gameObj.GetComponent<MeshFilter>());
                isSkinedMeshRenderer.Add(false);
            }
        }


        //Call children
        for (int i = 0; i < gameObj.transform.childCount; ++i)
        {
            PopulateLists(gameObj.transform.GetChild(i).gameObject);
        }
    }


    public void SetMaterial(ChromaColor color)
    {
        mat = colorObjMng.GetVoxelMaterial(color);
    }

    public void SetGreyMaterial()
    {
        mat = colorObjMng.GetVoxelGreyMaterial();
    }

    public void SetWhiteMaterial()
    {
        mat = colorObjMng.GetVoxelWhiteMaterial();
    }

    public bool SpawnColliderThisTime { set { spawnColliderThisTime = value; } }

    public void CalculateVoxelsGrid()
    {
        //New
        //1 Grid 1 Mesh
        if (!includeChildren)
        {
            aABCGrids = new List<VoxelizationServer.AABCGrid>();

            if (rend != null)
            {
                aABCGrids.Add(VoxelizationServer.Create1Grid1Object(transf, meshFilter.sharedMesh, rend, voxelSideSize, fillVoxelShell));
            }
            else if (sRend != null)
            {
                Mesh mesh = new Mesh();
                sRend.BakeMesh(mesh);
                aABCGrids.Add(VoxelizationServer.Create1Grid1Object(transf, mesh, sRend, voxelSideSize, fillVoxelShell));
            }
        }
        else
        {
            meshes.Clear();
            for (int i = 0; i < renderers.Count; ++i)
            {
                if (isSkinedMeshRenderer[i])
                {
                    Mesh mesh = new Mesh();
                    (renderers[i] as SkinnedMeshRenderer).BakeMesh(mesh);
                    meshes.Add(mesh);
                }
                else
                {
                    meshes.Add(meshFilters[i].sharedMesh);
                }
            }

            //1 Grid N Meshes
            if (!createMultipleGrids)
            {
                aABCGrids = new List<VoxelizationServer.AABCGrid>();
                aABCGrids.Add(VoxelizationServer.Create1GridNObjects(transforms, meshes, renderers, voxelSideSize, fillVoxelShell));
            }
            //N Grids N Meshes
            else
            {
                aABCGrids = VoxelizationServer.CreateNGridsNObjects(transforms, meshes, renderers, voxelSideSize, fillVoxelShell);
            }
        }
    }   

    /*public void SpawnVoxels()
    {
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
                                if(voxelController != null)
                                {
                                    //++total;
                                    Transform voxelTrans = voxelController.gameObject.transform;
                                    voxelTrans.position = cubeCenter;
                                    voxelTrans.rotation = Quaternion.identity;
                                    //voxelTrans.rotation = Random.rotation;
                                    voxelTrans.localScale = voxelScale;
                                    if(!randomMaterial)
                                    {
                                        voxelController.GetComponent<Renderer>().sharedMaterial = mat;
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
                if (spawnColliderThisTime)
                {                   
                    GameObject voxelCollider = voxelColliderPool.GetObject();
                    if (voxelCollider != null)
                    {
                        voxelCollider.transform.localScale = voxelColliderScale;
                        voxelCollider.transform.position = aABCGrid.GetCenter();
                    }
                }
            }
        }
        //Debug.Log("Spider spawned: " + total);

        //Return to default
        spawnColliderThisTime = spawnVoxelCollider;
    }*/

    public void SpawnVoxels()
    {
        VoxelizationInfo info = new VoxelizationInfo(aABCGrids, voxelScale, randomMaterial, mat, spawnColliderThisTime, voxelColliderScale);

        rsc.voxelizationMng.AddInfoToPendingSpawns(info);

        //Return to default
        spawnColliderThisTime = spawnVoxelCollider;
    }

    public void CalculateFakeVoxelsGrid()
    {
        int totalVoxels = Random.Range(fakeGridMinVoxels, fakeGridMaxVoxels + 1);
        short gridSideSize = (short)Mathf.Ceil(Mathf.Pow(totalVoxels, (1f / 3f)));

        fakeAABCGrid = new VoxelizationServer.AABCGrid(gridSideSize, gridSideSize, gridSideSize, voxelSideSize, transform.position);
        fakeAABCGrid.FillGridWithFixedNumber(totalVoxels);
        fakeAABCGrids.Add(fakeAABCGrid);
    }

    public void SpawnFakeVoxels()
    {
        Vector3 position = transform.position;
        position.y -= transform.localPosition.y;
        fakeAABCGrid.SetCenter(position);

        VoxelizationInfo info = new VoxelizationInfo(fakeAABCGrids, voxelScale, randomMaterial, mat, spawnColliderThisTime, voxelColliderScale);

        rsc.voxelizationMng.AddInfoToPendingSpawns(info);

        //Return to default
        spawnColliderThisTime = spawnVoxelCollider;
    }
}
