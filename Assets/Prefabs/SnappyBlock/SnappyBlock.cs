using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SnappyBlock : MonoBehaviour
{

    [SerializeField] private GameObject _snappyBlockContainerPrefab;
    [SerializeField] private BlockConnector _outputConnector;
    [SerializeField] private List<BlockConnector> _inputConnectors;
    private List<BlockOutputFinder> _outputFinders = new List<BlockOutputFinder>();

    void Start()
    {
        Debug.Log(_inputConnectors.Count);
        foreach (var inputConnector in _inputConnectors)
        {
            _outputFinders.Add(inputConnector.GetComponent<BlockOutputFinder>());
        }
    }

    public void OnSelected(XRSimpleInteractable xrSimapelInteractable)
    {
        var rawInteractor = xrSimapelInteractable.selectingInteractor;
        if (!(rawInteractor is XRRayInteractor)) return;

        XRRayInteractor rayInteractor = rawInteractor as XRRayInteractor;
        SnappyBlockContainer oldContainer = this.gameObject.GetComponentInParent<SnappyBlockContainer>();
        var newContainer = this.CreateNewContainer(rayInteractor);
        this.MoveSelfAndChildrenToOtherContainer(newContainer.transform);
        newContainer.GetComponent<SnappyBlockContainer>().WrapColliderAroundChildren();
        this.MakeUserGrabContainer(rayInteractor, newContainer);
    }

    private GameObject CreateNewContainer(XRRayInteractor interactor)
    {
        GameObject newContainer = Instantiate(_snappyBlockContainerPrefab, gameObject.transform.position, gameObject.transform.rotation);
        return newContainer;
    }

    private void MakeUserGrabContainer(XRRayInteractor interactor, GameObject container)
    {
        interactor.interactionLayerMask = ~0;
        FindObjectOfType<XRInteractionManager>().ForceSelect(interactor, container.GetComponent<XRGrabInteractable>());
    }

    public List<SnappyBlock> GetAllConnectedBlocksRecursive()
    {
        var connectedBlocks = new List<SnappyBlock>();
        foreach (var inputConnector in _inputConnectors)
        {
            connectedBlocks.Add(inputConnector.BlockAttachedTo);
            if (inputConnector.BlockConnectedTo != null)
                connectedBlocks.AddRange(inputConnector.BlockConnectedTo.GetAllConnectedBlocksRecursive());
        }
        return connectedBlocks;
    }

    public void SnapSelfAndChildren()
    {
        Debug.Log(this._outputConnector.BlockConnectedTo);
        if (this._outputConnector == null) return;
        if (this._outputConnector.BlockConnectedTo == null) return;
        this.gameObject.transform.position = this._outputConnector.CurrentConnection.transform.position;
        this.gameObject.transform.rotation = this._outputConnector.CurrentConnection.transform.rotation;
        foreach (var inputConnectors in this._inputConnectors)
        {
            if (inputConnectors.BlockConnectedTo == null) continue;
            inputConnectors.BlockConnectedTo.SnapSelfAndChildren();
        }
    }

    public void MoveSelfAndChildrenToOtherContainer(Transform parent)
    {
        if (this._outputConnector == null) return;
        if (this._outputConnector.BlockConnectedTo == null) return;
        this.NotifyContainerOfTransfere();
        this.transform.parent = parent;
        foreach (var inputConnectors in this._inputConnectors)
        {
            if (inputConnectors.BlockConnectedTo == null) continue;
            inputConnectors.BlockConnectedTo.MoveSelfAndChildrenToOtherContainer(parent);
        }
    }

    private void NotifyContainerOfTransfere()
    {
        GameObject container = this.transform.parent.gameObject;
        SnappyBlockContainer snappyBlockContainer = container.GetComponent<SnappyBlockContainer>();
        if (snappyBlockContainer == null) return;
        snappyBlockContainer.NotifyBlockDetachement(this);
    }

    public List<PossibleConnection> GetAllPossibleConnections()
    {
        var possibleConnections = new List<PossibleConnection>();
        foreach (var outputFinder in this._outputFinders)
        {
            possibleConnections.AddRange(outputFinder.GetAllPossibleConnections());
        }
        return possibleConnections;
    }

    
}
