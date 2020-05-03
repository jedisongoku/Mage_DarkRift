using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshRenderersInModel : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer[] skinnedRMeshenderers;
    [SerializeField] private MeshRenderer[] meshrenderers;
    
    public SkinnedMeshRenderer[] SkinnedMeshRenderers
    {
        get
        {
            return skinnedRMeshenderers;
        }
    }

    public MeshRenderer[] MeshRenderers
    {
        get
        {
            return meshrenderers;
        }
    }



}
