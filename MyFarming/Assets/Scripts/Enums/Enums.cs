public enum AnimationName
{
    idleDown,
    idleUp,
    idleRight,
    idleLeft,
    walkDown,
    walkUp,
    walkRight,
    walkLeft,
    runDown,
    runUp,
    runRight,
    runLeft,
    useToolDown,
    useToolUp,
    useToolRight,
    useToolLeft,
    swingToolDown,
    swingToolUp,
    swingToolRight,
    swingToolLeft,
    liftToolDown,
    liftToolUp,
    liftToolRight,
    liftToolLeft,
    holdToolDown,
    holdToolUp,
    holdToolRight,
    holdToolLeft,
    pickToolDown,
    pickToolUp,
    pickToolRight,
    pickToolLeft,
    count
}

public enum CharacterPartAnimator
{
    body, arms, hair, tool, hat, count
}

public enum PartVariantColor
{
    none, count
}

public enum PartVariantType
{
    none,
    sostener, 
    azada, 
    mai,
    hacha,
    guadaña,
    regadora,
    count
}

public enum InventoryLocation
{
    player,
    chest,
    count
}

public enum ToolEffect
{
    none, 
    watering
}

public enum Direction
{
    up,
    down,
    left,
    right
}

public enum ItemType
{
    Recurso,
    Semilla,
    Herramienta_Regar,
    Herramienta_Labrar,
    Herramienta_Cortar,
    Herramienta_Romper,
    Herramienta_Segar,
    Herramienta_Recoger,
    Escenario_Segable,
    mueble,
    none,
    count

}

public enum SceneName
{
    Scene1_Farm,
    Scene2_Field,
    Scene3_Cabin
}
public enum Season
{
    Primavera,
    Verano,
    Otoño,
    Invierno,
    none,
    count
}

public enum GridBoolProperty
{
    esExcabable,
    esSoltarItem,
    esSoltarMueble,
    esCamino,
    esNPCObstaculo
}

public enum HarvestActionEffect
{
    decidiousLeavesFalling,
    pineConesFalling,
    choppingTreeTrunk,
    breakingStone,
    reaping,
    none
}

public enum Facing
{
    none,
    front,
    back,
    right
}