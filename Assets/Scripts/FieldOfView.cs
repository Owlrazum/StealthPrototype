using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class FieldOfView : MonoBehaviour
{
    [SerializeField]
    private float angle;
    [SerializeField]
    private int segments;
    [SerializeField]
    private float height = 1;

    private int numberOfLayers = 2;

    private bool isActive = true;

    float angleSegment;
    float layerHeight;
    int circumferenceVerticesAmount;
    int nVerticesPerLayer;
    int totalVertices;
    int index;

    [SerializeField]
    private float viewDistance;
    public float ViewDistance { get { return viewDistance; } set { } }
    private bool isCleared = false;

    private bool isRunning = false;

    private GirlLogic logic;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    Mesh baseMesh;
    Vector3[] baseVertices = new Vector3[0];
    int[] baseTriangles = new int[0];

    Mesh currentMesh;
    Vector3[] vertices = new Vector3[0];
    int[] triangles = new int[0];

    private void Start()
    {
        logic = transform.parent.GetComponent<GirlLogic>();
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        EventSystem.Instance.OnPlayerSpotted += PlayerSpotted;
        EventSystem.Instance.OnPlayerCaptured += PlayerCaptured;
        //EventSystem.Instance.OnVictoryObjectPickedUp += Activate;

        GirlLogic girl = transform.parent.GetComponent<GirlLogic>();
        isActive = !girl.IsSleeping() && !girl.IsIdle();

        angleSegment = angle / segments;
        layerHeight = height / (numberOfLayers - 1);
        circumferenceVerticesAmount = segments + 1;
        nVerticesPerLayer = 1 + circumferenceVerticesAmount;
        totalVertices = nVerticesPerLayer * numberOfLayers;

        Initialize();
    }

    private void Initialize()
    {
        baseVertices = new Vector3[totalVertices];
        vertices = new Vector3[baseVertices.Length + 8]; // the last 8 are for intersection

        int topBottomCount = segments * 3 * numberOfLayers;
        int sideCount = 3 * 2 * (2 + segments) * (numberOfLayers - 1);
        int indicesCount = topBottomCount + sideCount;

        baseTriangles = new int[indicesCount];
        triangles = new int[baseTriangles.Length + 24]; // the last 24 plus 24 somewhere else are for intersection

        baseMesh = new Mesh();
        baseMesh.Clear();
        GenerateBaseMeshData();
        baseVertices.CopyTo(vertices, 0);
        baseTriangles.CopyTo(baseTriangles, 0);

        //CheckMeshData();
        baseMesh.vertices = baseVertices;
        baseMesh.triangles = baseTriangles;
        baseMesh.RecalculateNormals();
        meshFilter.mesh = baseMesh;
        meshCollider.sharedMesh = baseMesh;
        isRunning = true;
    }

    public void Activate()
    {
        isActive = true;
    }

    private void Update()
    {
        if (isActive)
        {
            if (isCleared)
            {
                Initialize();
                return;
            }
           // angleSegment = angle / segments;
           // layerHeight = height / (numberOfLayers - 1);
          ///  circumferenceVerticesAmount = segments + 1;
           // nVerticesPerLayer = 1 + circumferenceVerticesAmount;
           // totalVertices = nVerticesPerLayer * numberOfLayers;

            currentMesh = new Mesh();
            currentMesh.Clear();
            UpdateMeshData();
            currentMesh.vertices = vertices;
            currentMesh.triangles = baseTriangles;
            currentMesh.RecalculateNormals();
            meshFilter.mesh = currentMesh;
        } else
        {
            meshFilter.mesh.Clear();
            isCleared = true;
        }
    }

    #region MeshGeneration
    private void CheckMeshData()
    {
        string message = "";
        for (int i = 0; i < baseTriangles.Length; i += 3)
        {
            message += baseTriangles[i] + " ";
            message += baseTriangles[i + 1] + " ";
            message += baseTriangles[i + 2] + " \n";
        }
        //Debug.Log(message);
    }
    private void GenerateBaseMeshData()
    {
        index = 0;
        ComputeVertices();
        index = 0;
        ComputeTopBottomIndices();
        //Debug.Log("before angle " + index);
        ComputeAngleTriangles();
        //Debug.Log(index);
        ComputeCircumferenceTriangles();
    }
    private void UpdateMeshData()
    {
        UpdateVertices();
    }
    private void ComputeVertices()
    {
        for (int n = 0; n < numberOfLayers; n++)
        {
            index = nVerticesPerLayer * n;
            float yPos = n * layerHeight;
            baseVertices[nVerticesPerLayer * n] = new Vector3(0, yPos, 0);
            index++;
            for (int i = 0; i < circumferenceVerticesAmount; i++)
            {
                Vector3 pos = new Vector3(
                    Mathf.Sin((-angle / 2 + angleSegment * i) * Mathf.Deg2Rad) * viewDistance,
                    yPos,
                    Mathf.Cos((-angle / 2 + angleSegment * i) * Mathf.Deg2Rad) * viewDistance
                );
                baseVertices[index++] = pos;
                //Debug.DrawLine(transform.TransformPoint(new Vector3(0, 0, 0)), transform.TransformPoint(pos), Color.black, 100f);
            }
        }
    }

    private Ray ray;
    private int beginIndex = -1;
    private int endIndex = -1;
    private void UpdateVertices()
    {
        for (int x = 0; x < 2; x++)
        {
            int index = x * nVerticesPerLayer + 1;
            for (int i = 1; i < circumferenceVerticesAmount + 1; i++)
            {
                Vector3 origin = transform.TransformPoint(vertices[x * nVerticesPerLayer]);
                Vector3 direction = transform.TransformPoint(vertices[index]) - origin;
                ray = new Ray(origin, direction);
                RaycastHit rayHit;
                int layerMask = 1 << 9;
                layerMask = ~layerMask;
                if (Physics.Raycast(ray, out rayHit, viewDistance, layerMask, QueryTriggerInteraction.Ignore))
                {
                    //Debug.Log("Modifying vertice");
                    Vector3 newPos = transform.InverseTransformPoint(rayHit.point);
                    //Debug.Log("vertex modified " + newPos);
                    vertices[index].x = newPos.x;
                    vertices[index].z = newPos.z;
                }
                else if (baseMesh.vertices[index] != vertices[index])
                {
                    //Debug.Log("Restoring vertice");
                    vertices[index] = baseVertices[index];
                }
                index++;
            }
        }
        string message = "";
        for (int i = 0; i < vertices.Length; i++)
        {
            message += vertices[i] + "\n";
        }
        //Debug.Log(message);
    }
    private void ComputeTopBottomIndices()
    {
        for (int i = 0; i < 2; i++) // 0 - bottom; 1 - top
        {
            int baseIndex = nVerticesPerLayer * (numberOfLayers - 1) * i;
            int circumferenceIndice = baseIndex + 2;
            int indexCount = segments * 3 + index;
            while (index < indexCount)
            {
                baseTriangles[index++] = baseIndex;
                baseTriangles[index++] = circumferenceIndice + i * (-1);
                baseTriangles[index++] = circumferenceIndice + i + (-1);
                circumferenceIndice++;
            }
        }
    }
    private void ComputeAngleTriangles()
    {
        for (int n = 1; n < numberOfLayers; n++)
        {
            int centerBottom = nVerticesPerLayer * (n - 1);
            int centerTop = centerBottom + nVerticesPerLayer;
            int leftBottom = centerBottom + 1;
            int leftTop = centerTop + 1;
            int rightBottom = leftBottom + segments;
            int rightTop = leftTop + segments;
            baseTriangles[index++] = centerBottom;
            baseTriangles[index++] = leftBottom;
            baseTriangles[index++] = leftTop;

            baseTriangles[index++] = leftTop;
            baseTriangles[index++] = centerTop;
            baseTriangles[index++] = centerBottom;

            baseTriangles[index++] = centerBottom;
            baseTriangles[index++] = centerTop;
            baseTriangles[index++] = rightTop;

            baseTriangles[index++] = rightTop;
            baseTriangles[index++] = rightBottom;
            baseTriangles[index++] = centerBottom;
        }
    }
    private void ComputeCircumferenceTriangles()
    {
        for (int n = 1; n < numberOfLayers; n++)
        {
            int leftBottom = nVerticesPerLayer * (n - 1) + 1;
            int leftTop = leftBottom + nVerticesPerLayer;
            int rightTop = leftTop + 1;
            int rightBottom = leftBottom + 1;
            for (int i = 0; i < segments; i++)
            {
                //Debug.Log("cycle " + n + " " + index);
                baseTriangles[index++] = leftBottom;
                baseTriangles[index++] = rightBottom;
                baseTriangles[index++] = rightTop;
                baseTriangles[index++] = rightTop;
                baseTriangles[index++] = leftTop;
                baseTriangles[index++] = leftBottom;
                leftBottom = rightBottom;
                leftTop = rightTop;
                rightBottom++;
                rightTop++;
            }
        }
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<PlayerLogic>() != null)
        {
            logic.ShouldCheckPlayerVisibility(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerLogic>() != null)
        {
            logic.ShouldCheckPlayerVisibility(false);
        }
    }

    private void PlayerCaptured()
    {
        gameObject.SetActive(false);
    }

    private void PlayerSpotted()
    {
        gameObject.SetActive(false);
    }


    public void SetActive(bool isArg)
    {
        bool isActive = isArg;
    }

    private void OnDisable()
    {

        //Debug.Log("Field disabled");
    }

    /*    private void OnDrawGizmos()
    {
        if (isRunning)
        {
            //Debug.Log("drawing");
            for (int i = 0; i < vertices.Length; i++)
            {
                //Gizmos.DrawCube(transform.TransformPoint(vertices[i]), new Vector3(0.5f,0.5f,0.5f));
            }
        }
    }*/

}
