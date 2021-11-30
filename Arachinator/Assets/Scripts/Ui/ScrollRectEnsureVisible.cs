using UnityEngine;
 using UnityEngine.UI;
 using UnityEngine.EventSystems;

 [RequireComponent(typeof(ScrollRect))]
 public class ScrollRectEnsureVisible : MonoBehaviour
 {
     RectTransform contentPanel;
     RectTransform selectedRectTransform;
     GameObject lastSelected;

     void Update()
     {
         if(contentPanel == null) contentPanel = GetComponent<ScrollRect>().content;

         var selected = EventSystem.current.currentSelectedGameObject;

         if (selected == null)
             return;
         if (selected.transform.parent != contentPanel.transform)
             return;
         if (selected == lastSelected)
             return;

         selectedRectTransform = selected.GetComponent<RectTransform>();
         contentPanel.anchoredPosition = new Vector2(contentPanel.anchoredPosition.x, - (selectedRectTransform.localPosition.y) - (selectedRectTransform.rect.height/2));

         lastSelected = selected;
     }
 }
