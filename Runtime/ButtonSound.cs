using System;
using Sonity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BIG.Unity.Sonity
{
    [RequireComponent(typeof(Button))]
    public class ButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private SoundEvent _onButtonHoverEnter;
        [SerializeField] private SoundEvent _onButtonHoverExit;
        [SerializeField] private SoundEvent _onButtonClick;

        public void OnPointerEnter(PointerEventData eventData)
        {
           if(_button.interactable && _onButtonHoverEnter != null) SoundManager.Play(_onButtonHoverEnter);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(_button.interactable && _onButtonHoverExit != null) SoundManager.Play(_onButtonHoverExit);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(_button.interactable && _onButtonClick != null) SoundManager.Play(_onButtonClick);
        }

        private void OnValidate()
        {
            if (_button == null)
                _button = GetComponent<Button>();
        }
    }
}
