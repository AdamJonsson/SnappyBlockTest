using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SnappyBlock : MonoBehaviour
{
    private XRGrabInteractable _xrGrabInteracable;
    [SerializeField] private XRSocketInteractor _xrSocketInteractor;

    List<SnappyBlock> _connectedBlocks;

    void Awake()
    {
        _xrGrabInteracable = this.GetComponent<XRGrabInteractable>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnNewCubeConnected()
    {
        var cubeThatConnected = _xrSocketInteractor.selectTarget.gameObject;
        print(this.gameObject.transform.position);
        print(cubeThatConnected.transform.position);
    }
}
