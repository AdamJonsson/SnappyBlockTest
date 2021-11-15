using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class SnappyBlockContainer : MonoBehaviour
{
    [SerializeField] private LineRenderer _connectionSuggestionLinePrefab;

    private List<SnappyBlock> children = new List<SnappyBlock>();
    private LineRenderer _connectionSuggestionLine;

    // Start is called before the first frame update
    void Start()
    {
        WrapColliderAroundChildren();

        if (!Application.isPlaying) return;
        this.TrackAllChildrenAtStart();
        var line = Instantiate(this._connectionSuggestionLinePrefab, this.transform);
        line.gameObject.SetActive(false);
        this._connectionSuggestionLine = line;
        // MakeBoxColliderContainChildren();
    }

    void Update()
    {
        // MakeBoxColliderContainChildren();
        // if (Application.isEditor) {
        //     WrapColliderAroundChildren();
        // }
        if (!Application.isPlaying) return;
        this.RenderConnectionLine();
    }

    public void OnCubeDiselected()
    {
        SnappyBlockContainer[] allBlockContainers = FindObjectsOfType<SnappyBlockContainer>();
        foreach (var container in allBlockContainers)
        {
            container.ConnectClosestConnection();
        }
    }

    private void TrackAllChildrenAtStart()
    {
        this.children = this.gameObject.GetComponentsInChildren<SnappyBlock>().ToList();
    }

    public void NotifyBlockDetachement(SnappyBlock blockBeingDetached)
    {
        this.WrapColliderAroundChildren();
        var allBlocksToRemove = blockBeingDetached.GetAllConnectedBlocksRecursive();
        this.children.Remove(blockBeingDetached);
        foreach (var child in allBlocksToRemove)
        {
            this.children.Remove(child);
        }
    }

    public void NotifyBlockAttachement(SnappyBlock blockBeingAttach)
    {
        this.children.Add(blockBeingAttach);
        this.WrapColliderAroundChildren();
    }

    public List<PossibleConnection> GetAllPossibleConnections()
    {
        var possibleConnections = new List<PossibleConnection>();
        if (this.children.Count == 0) return possibleConnections;
        foreach (var child in this.children)
        {
            possibleConnections.AddRange(child.GetAllPossibleConnections());
        }
        return possibleConnections;
    }

    public PossibleConnection GetClosestPossibleConnection()
    {
        List<PossibleConnection> possibleConnections = GetAllPossibleConnections();
        if (possibleConnections.Count == 0) return null;
        PossibleConnection closestLineSoFar = possibleConnections[0];
        float minDistance = closestLineSoFar.Distance;
        foreach (var possibleConnection in possibleConnections)
        {
            if (possibleConnection.Distance < minDistance)
            {
                minDistance = possibleConnection.Distance;
                closestLineSoFar = possibleConnection;
            }
        }
        return closestLineSoFar;
    }

    public void ConnectClosestConnection()
    {
        PossibleConnection connection = this.GetClosestPossibleConnection();
        if (connection == null) return;
        var blockToConnect = connection.Output.BlockAttachedTo;
        connection.Input.ConnectToConnector(connection.Output);
        connection.Output.ConnectToConnector(connection.Input);
        blockToConnect.MoveSelfAndChildrenToOtherContainer(this.transform);
        blockToConnect.SnapSelfAndChildren();
        this.NotifyBlockAttachement(blockToConnect);
        this.WrapColliderAroundChildren();
    }

    void RenderConnectionLine()
    {
        var possibleConnection = this.GetClosestPossibleConnection();
        if (possibleConnection == null)
        {
            this._connectionSuggestionLine.gameObject.SetActive(false);
            return;
        }

        this._connectionSuggestionLine.gameObject.SetActive(true);
        this._connectionSuggestionLine.SetPositions(new List<Vector3>(){
            possibleConnection.Input.HoverLineAttachementPoint.position,
            possibleConnection.Output.HoverLineAttachementPoint.position
        }.ToArray());
    }

    public void WrapColliderAroundChildren()
    {
        var pos = gameObject.transform.localPosition;
        var rot = gameObject.transform.localRotation;
        var scale = gameObject.transform.localScale;

        // need to clear out transforms while encapsulating bounds
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.identity;
        gameObject.transform.localScale = Vector3.one;

        // start with root object's bounds
        var bounds = new Bounds(Vector3.zero, Vector3.zero);
        if (gameObject.transform.TryGetComponent<Renderer>(out var mainRenderer))
        {
            // as mentioned here https://forum.unity.com/threads/what-are-bounds.480975/
            // new Bounds() will include 0,0,0 which you may not want to Encapsulate
            // because the vertices of the mesh may be way off the model's origin
            // so instead start with the first renderer bounds and Encapsulate from there
            bounds = mainRenderer.bounds;
        }

        var descendants = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform desc in descendants)
        {
            if (desc.TryGetComponent<Renderer>(out var childRenderer))
            {
                // use this trick to see if initialized to renderer bounds yet
                // https://answers.unity.com/questions/724635/how-does-boundsencapsulate-work.html
                var childBounds = childRenderer.bounds;
                childBounds.size *= 1.1f;
                if (bounds.extents == Vector3.zero)
                    bounds = childBounds;
                bounds.Encapsulate(childBounds);
            }
        }

        var boxCol = gameObject.GetComponent<BoxCollider>();
        boxCol.center = bounds.center - gameObject.transform.position;
        boxCol.size = bounds.size;

        // restore transforms
        gameObject.transform.localPosition = pos;
        gameObject.transform.localRotation = rot;
        gameObject.transform.localScale = scale;
    }

}
