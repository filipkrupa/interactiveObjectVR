using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;


    public abstract class InteractableObject : MonoBehaviour
{
    public abstract void OnLook();
    public abstract void OnClick();
    public abstract void OnLookAway();

    //public abstract void myStart();  dont delete


    //ALL THIS IS JUST FOR THE HIGHLIGHTING
    public bool start = false;
    public bool stop = false;

    protected MeshRenderer[] highlightRenderers;
    protected MeshRenderer[] existingRenderers;
    protected GameObject highlightHolder;
    protected SkinnedMeshRenderer[] highlightSkinnedRenderers;
    protected SkinnedMeshRenderer[] existingSkinnedRenderers;
    protected static Material highlightMat;
    [Tooltip("An array of child gameObjects to not render a highlight for. Things like transparent parts, vfx, etc.")]
    public GameObject[] hideHighlight;

    [Tooltip("Higher is better")]
    public int hoverPriority = 0;

    [System.NonSerialized]
    public Hand attachedToHand;

    [System.NonSerialized]
    public List<Hand> hoveringHands = new List<Hand>();
    public Hand hoveringHand
    {
        get
        {
            if (hoveringHands.Count > 0)
                return hoveringHands[0];
            return null;
        }
    }


    public bool isDestroying { get; protected set; }
    public bool isHovering { get; protected set; }
    public bool wasHovering { get; protected set; }

    protected virtual void Start()
    {
        highlightMat = (Material)Resources.Load("SteamVR_HoverHighlight", typeof(Material));

        if (highlightMat == null)
            Debug.LogError("<b>[SteamVR Interaction]</b> Hover Highlight Material is missing. Please create a material named 'SteamVR_HoverHighlight' and place it in a Resources folder", this);

        MyStart();
    }

    protected virtual bool ShouldIgnoreHighlight(Component component)
    {
        return ShouldIgnore(component.gameObject);
    }

    protected virtual bool ShouldIgnore(GameObject check)
    {
        for (int ignoreIndex = 0; ignoreIndex < hideHighlight.Length; ignoreIndex++)
        {
            if (check == hideHighlight[ignoreIndex])
                return true;
        }

        return false;
    }

    protected virtual void CreateHighlightRenderers()
    {
        existingSkinnedRenderers = this.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        highlightHolder = new GameObject("Highlighter");
        highlightSkinnedRenderers = new SkinnedMeshRenderer[existingSkinnedRenderers.Length];

        for (int skinnedIndex = 0; skinnedIndex < existingSkinnedRenderers.Length; skinnedIndex++)
        {
            SkinnedMeshRenderer existingSkinned = existingSkinnedRenderers[skinnedIndex];

            if (ShouldIgnoreHighlight(existingSkinned))
                continue;

            GameObject newSkinnedHolder = new GameObject("SkinnedHolder");
            newSkinnedHolder.transform.parent = highlightHolder.transform;
            SkinnedMeshRenderer newSkinned = newSkinnedHolder.AddComponent<SkinnedMeshRenderer>();
            Material[] materials = new Material[existingSkinned.sharedMaterials.Length];
            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                materials[materialIndex] = highlightMat;
            }

            newSkinned.sharedMaterials = materials;
            newSkinned.sharedMesh = existingSkinned.sharedMesh;
            newSkinned.rootBone = existingSkinned.rootBone;
            newSkinned.updateWhenOffscreen = existingSkinned.updateWhenOffscreen;
            newSkinned.bones = existingSkinned.bones;

            highlightSkinnedRenderers[skinnedIndex] = newSkinned;
        }

        MeshFilter[] existingFilters = this.GetComponentsInChildren<MeshFilter>(true);
        existingRenderers = new MeshRenderer[existingFilters.Length];
        highlightRenderers = new MeshRenderer[existingFilters.Length];

        for (int filterIndex = 0; filterIndex < existingFilters.Length; filterIndex++)
        {
            MeshFilter existingFilter = existingFilters[filterIndex];
            MeshRenderer existingRenderer = existingFilter.GetComponent<MeshRenderer>();

            if (existingFilter == null || existingRenderer == null || ShouldIgnoreHighlight(existingFilter))
                continue;

            GameObject newFilterHolder = new GameObject("FilterHolder");
            newFilterHolder.transform.parent = highlightHolder.transform;
            MeshFilter newFilter = newFilterHolder.AddComponent<MeshFilter>();
            newFilter.sharedMesh = existingFilter.sharedMesh;
            MeshRenderer newRenderer = newFilterHolder.AddComponent<MeshRenderer>();

            Material[] materials = new Material[existingRenderer.sharedMaterials.Length];
            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                materials[materialIndex] = highlightMat;
            }
            newRenderer.sharedMaterials = materials;

            highlightRenderers[filterIndex] = newRenderer;
            existingRenderers[filterIndex] = existingRenderer;
        }
    }

    protected virtual void UpdateHighlightRenderers()
    {
        if (highlightHolder == null)
            return;

        for (int skinnedIndex = 0; skinnedIndex < existingSkinnedRenderers.Length; skinnedIndex++)
        {
            SkinnedMeshRenderer existingSkinned = existingSkinnedRenderers[skinnedIndex];
            SkinnedMeshRenderer highlightSkinned = highlightSkinnedRenderers[skinnedIndex];

            if (existingSkinned != null && highlightSkinned != null && attachedToHand == false)
            {
                highlightSkinned.transform.position = existingSkinned.transform.position;
                highlightSkinned.transform.rotation = existingSkinned.transform.rotation;
                highlightSkinned.transform.localScale = existingSkinned.transform.lossyScale;
                highlightSkinned.localBounds = existingSkinned.localBounds;
                highlightSkinned.enabled = isHovering && existingSkinned.enabled && existingSkinned.gameObject.activeInHierarchy;

                int blendShapeCount = existingSkinned.sharedMesh.blendShapeCount;
                for (int blendShapeIndex = 0; blendShapeIndex < blendShapeCount; blendShapeIndex++)
                {
                    highlightSkinned.SetBlendShapeWeight(blendShapeIndex, existingSkinned.GetBlendShapeWeight(blendShapeIndex));
                }
            }
            else if (highlightSkinned != null)
                highlightSkinned.enabled = false;

        }

        for (int rendererIndex = 0; rendererIndex < highlightRenderers.Length; rendererIndex++)
        {
            MeshRenderer existingRenderer = existingRenderers[rendererIndex];
            MeshRenderer highlightRenderer = highlightRenderers[rendererIndex];

            if (existingRenderer != null && highlightRenderer != null && attachedToHand == false)
            {
                highlightRenderer.transform.position = existingRenderer.transform.position;
                highlightRenderer.transform.rotation = existingRenderer.transform.rotation;
                highlightRenderer.transform.localScale = existingRenderer.transform.lossyScale;
                highlightRenderer.enabled = isHovering && existingRenderer.enabled && existingRenderer.gameObject.activeInHierarchy;
            }
            else if (highlightRenderer != null)
                highlightRenderer.enabled = false;
        }
    }

    public void Highlight()
    {
        wasHovering = isHovering;
        isHovering = true;

        if (!wasHovering)
        {
            CreateHighlightRenderers();
            UpdateHighlightRenderers();
        }
    }

    public void removeHighlight()
    {
        wasHovering = isHovering;
        if (hoveringHands.Count == 0)
        {
            isHovering = false;

            if (highlightHolder != null)
                Destroy(highlightHolder);
        }
    }

    protected virtual void Update()
    {
        UpdateHighlightRenderers();
        if (isHovering == false && highlightHolder != null)
            Destroy(highlightHolder);

        if (start)
        {
            start = false;
            Highlight();
        }

        if (stop)
        {
            stop = false;
            removeHighlight();
        }

        MyUpdate();
    }

    public virtual void MyUpdate() { }
    public virtual void MyStart() { }
}

