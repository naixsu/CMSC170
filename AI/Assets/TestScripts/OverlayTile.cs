using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayTile : MonoBehaviour
{
    #region PATHFINDING VARS
    public int _G;
    public int _H;
    public int _F;
    // public int _F { get { return _G + _H; } }
    public bool isBlocked;
    public OverlayTile previous;

    public Vector3Int gridLocation;
    public Vector2Int grid2DLocation { get { return new Vector2Int(gridLocation.x, gridLocation.y); } }
    #endregion


    [SerializeField] private Sprite overlayTile;
    [SerializeField] private Sprite plantedSprite;
    [SerializeField] private Sprite growth1;
    [SerializeField] private Sprite growth2;
    [SerializeField] private Sprite growth3;
    [SerializeField] private Sprite growth4;
    [SerializeField] private Sprite harvested;

    public bool isTilled;
    public bool hasSeed;
    public bool isFullGrown;
    public bool isHarvested;
    private Coroutine coroutinePlantGrowth;
    
    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetMouseButtonDown(0))
        {
            HideTile();
        }*/
    }

    public void CalculateFCost()
    {
        _F = _G + _H;
    }

    //
    // pretty self explanatory stuff here
    // 
    public void PlantSeed()
    {
        if (!hasSeed)
        {
            this.hasSeed = true;
            this.isHarvested = false;
            this.isFullGrown = false;
            this.GetComponent<SpriteRenderer>().sprite = plantedSprite;
            coroutinePlantGrowth = StartCoroutine(PlantGrowth());
            Debug.Log("Planted seed at tile " + this.gameObject.transform.position.x + " " + this.gameObject.transform.position.y);
        }
    }

    public void HarvestCrop()
    {
        if (isFullGrown)
        {
            this.GetComponent<SpriteRenderer>().sprite = harvested;
            this.isHarvested = true;
            Debug.Log("Harvested crop at tile " + this.gameObject.transform.position.x + " " + this.gameObject.transform.position.y);
        }
    }

    public IEnumerator PlantGrowth()
    {
        yield return new WaitForSeconds(3f);
        this.GetComponent<SpriteRenderer>().sprite = growth1;
        yield return new WaitForSeconds(3f);
        this.GetComponent<SpriteRenderer>().sprite = growth2;
        yield return new WaitForSeconds(3f);
        this.GetComponent<SpriteRenderer>().sprite = growth3;
        yield return new WaitForSeconds(3f);
        this.GetComponent<SpriteRenderer>().sprite = growth4;
        this.isFullGrown = true;
    }

    public void ShowTile(float alpha)
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
    }

    public void HighlightTile()
    {
        Transform childTransform = transform.GetChild(0);
        childTransform.gameObject.SetActive(true);
    }

    public void HideHighlightTile()
    {
        Transform childTransform = transform.GetChild(0);
        childTransform.gameObject.SetActive(false);
    }

    public void HideTile()
    {
        if (!isTilled)
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
    }

    public void TillTile()
    {
        if (!isTilled)
        {
            this.isTilled = true;
            Debug.Log("Tilled tile at " + this.gameObject.transform.position.x + " " + this.gameObject.transform.position.y);
            this.ShowTile(1f);
        }     
    }

    public void UntillTile()
    {
        if (isTilled)
        {
            this.isTilled = false;
            Debug.Log("Untilled tile at " + this.gameObject.transform.position.x + " " + this.gameObject.transform.position.y);
            this.HideTile();
        }
    }
}
