[System.Serializable]
public class GridPropertyDetails
{
    public int gridX;
    public int gridY;
    public bool esExcavable = false;
    public bool esSoltarItem = false;
    public bool esSoltarMueble = false;
    public bool esCamino = false;
    public bool esNPCObstaculo = false;
    public int daysSinceDug = -1;
    public int daysSinceWatered = -1;
    public int seedItemCode = -1;
    public int growthDays = -1;
    public int daysSinceLastHarvest = -1;

    public GridPropertyDetails()
    {

    }
}
