using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockConnector : MonoBehaviour
{
    [SerializeField] private SnappyBlock _blockAttachedTo;
    private BlockConnector _currentConnection;

    [SerializeField] private Transform _hoverLineAttachementPoint;

    public Transform HoverLineAttachementPoint {
        get => this._hoverLineAttachementPoint;
    }

    public SnappyBlock BlockAttachedTo {
        get => this._blockAttachedTo;
    }

    public SnappyBlock BlockConnectedTo {
        get {
            if (this._currentConnection == null) return null;
            return this._currentConnection.BlockAttachedTo;
        }
    }

    public BlockConnector CurrentConnection {
        get => this._currentConnection;
    }

    public void ConnectToConnector(BlockConnector connection)
    {
        this._currentConnection = connection;
    }

    public void RemoveBlockConnection()
    {
        this._currentConnection = null;
    }
}
