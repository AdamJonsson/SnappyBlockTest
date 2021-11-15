using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BlockConnector))]
public class BlockOutputFinder : MonoBehaviour
{
    private List<BlockConnector> _possibleConnections = new List<BlockConnector>();

    private BlockConnector _blockConnector;

    void Start()
    {
        this._blockConnector = GetComponent<BlockConnector>();
    }

    void OnTriggerEnter(Collider other)
    {
        var output = GetPossibleOutputFromCollider(other);
        if (output == null) return;
        _possibleConnections.Add(output);
    }

    void OnTriggerExit(Collider other)
    {
        var output = GetPossibleOutputFromCollider(other);
        if (output == null) return;
        _possibleConnections.Remove(output);
    }

    private BlockConnector GetPossibleOutputFromCollider(Collider other)
    {
        if (_blockConnector.BlockConnectedTo != null) return null;
        if (other.tag != "BlockOutput") return null;
        var output = other.gameObject.GetComponent<BlockConnector>();
        if (output.BlockConnectedTo != null) return null;
        return output;
    }

    public BlockConnector GetClosestPossibleConnection()
    {
        if (_possibleConnections.Count == 0) return null;
        float minDistance = 0.0f;
        BlockConnector closestLineSoFar = _possibleConnections[0];
        foreach (var possibleConnection in _possibleConnections)
        {
            float distance = Vector3.Distance(possibleConnection.transform.position, this.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestLineSoFar = possibleConnection;
            }
        }
        return closestLineSoFar;
    }

    public IEnumerable<PossibleConnection> GetAllPossibleConnections()
    {
        if (this._blockConnector.BlockConnectedTo != null) yield break;
        foreach (var possibleConnection in _possibleConnections)
        {
            float distance = Vector3.Distance(possibleConnection.transform.position, this.transform.position);
            yield return new PossibleConnection(){
                Distance = distance,
                Input = this._blockConnector,
                Output = possibleConnection
            };
        }
    }

    void Update()
    {
        if (this._blockConnector.BlockConnectedTo != null)
            Debug.DrawLine(this.transform.position, this._blockConnector.BlockConnectedTo.transform.position, Color.red);
    }

}
