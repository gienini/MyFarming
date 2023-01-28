using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationOverrides : MonoBehaviour
{

    [SerializeField] private GameObject character = null;
    //Aqui arrastraremos las animaciones en la UI de unity
    [SerializeField] private SO_AnimationType[] soAnimationTypeArray = null;

    public Dictionary<AnimationClip, SO_AnimationType> animationTypeDictionariyByAnimation;
    public Dictionary<string, SO_AnimationType> animationTypeDictionariyByCompositeAttributeKey;

    private void Start()
    {
        //Se inicializan los diccionarios para tener la informacion ordenada y accesible
        animationTypeDictionariyByAnimation = new Dictionary<AnimationClip, SO_AnimationType>();
        foreach(SO_AnimationType item in soAnimationTypeArray)
        {
            animationTypeDictionariyByAnimation.Add(item.animationClip, item);
        }

        animationTypeDictionariyByCompositeAttributeKey = new Dictionary<string, SO_AnimationType>();
        foreach (SO_AnimationType item in soAnimationTypeArray)
        {
            string key = item.characterPart.ToString() + item.partVariantColor.ToString() + item.partVariantType.ToString() + item.animationName.ToString();
            animationTypeDictionariyByCompositeAttributeKey.Add(key, item);
        }

    }


    public void ApplyCharacterCustomisationParameters(List<CharacterAttribute> characterAttributesList)
    {
        //Iteramos los characterAtributes y seteamos el animationOverrideController para cada uno
        foreach(CharacterAttribute characterAttribute in characterAttributesList)
        {
            Animator currentAnimator = null;
            List<KeyValuePair<AnimationClip, AnimationClip>> animsKeyValuePairList = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            string animatorSOAssetName = characterAttribute.characterPart.ToString();


            //Buscamos los animadores en la escena que correspondan al animatorType del ScriptableObject (SO)
            Animator[] animatorsArray = character.GetComponentsInChildren<Animator>();

            foreach(Animator animator in animatorsArray)
            {
                if (animator.name == animatorSOAssetName)
                {
                    currentAnimator = animator;
                    break;
                }
            }

            AnimatorOverrideController aoc = new AnimatorOverrideController(currentAnimator.runtimeAnimatorController);
            List<AnimationClip> animationsList = new List<AnimationClip>(aoc.animationClips);

            foreach (AnimationClip animationClip in animationsList)
            {
                SO_AnimationType sO_AnimationType;
                bool foundAnimation = animationTypeDictionariyByAnimation.TryGetValue(animationClip, out sO_AnimationType);

                if (foundAnimation)
                {
                    string key = characterAttribute.characterPart.ToString() + characterAttribute.partVariantColor.ToString()
                        + characterAttribute.partVariantType.ToString() + sO_AnimationType.animationName.ToString();

                    SO_AnimationType swapSO_AnimationType;
                    bool foundSwapAnimation = animationTypeDictionariyByCompositeAttributeKey.TryGetValue(key, out swapSO_AnimationType);

                    if ( foundSwapAnimation)
                    {
                        AnimationClip swapAnimationClip = swapSO_AnimationType.animationClip;
                        animsKeyValuePairList.Add(new KeyValuePair<AnimationClip, AnimationClip>(animationClip, swapAnimationClip));
                    }
                }
            }
            //Aplicamos las actualizaciones en los animators con el nuevo controlador que acabamos de crear
            aoc.ApplyOverrides(animsKeyValuePairList);
            currentAnimator.runtimeAnimatorController = aoc;
        }
    }
}
