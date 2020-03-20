using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshRenderersInModel : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer[] skinnedRMeshenderers;
    [SerializeField] private MeshRenderer[] Meshrenderers;
    // Start is called before the first frame update
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
            return Meshrenderers;
        }
    }
}
