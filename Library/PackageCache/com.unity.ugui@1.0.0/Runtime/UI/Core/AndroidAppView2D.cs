using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.EventSystems;
#if UNITY_EDITOR || ENABLE_ANDROID_APPVIEW
using UnityEngine.Experimental.Android.AppView;
#endif
using System.Runtime.CompilerServices;

#if UNITY_EDITOR || ENABLE_ANDROID_APPVIEW
[assembly: InternalsVisibleTo("UnityEditor.UI")]
#endif
namespace UnityEngine.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("Android App View (Experimental)/Android App View 2D")]
    public class AndroidAppView2D : MaskableGraphic
#if ENABLE_ANDROID_APPVIEW
        , IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
        , IBeginDragHandler, IEndDragHandler, IDragHandler
        , IAndroidAppViewEventCallback
#endif
    {
#if UNITY_EDITOR || ENABLE_ANDROID_APPVIEW
        [SerializeField]
        private AndroidAppViewSettings m_AndroidAppViewSettings;
#endif
#if ENABLE_ANDROID_APPVIEW
        private AndroidAppViewController m_AndroidAppViewController;
        private RectTransform m_TouchInputPlane;
        private bool m_IsTouchInsideView;
        private bool m_Dragging;
        private Vector2 m_LastValidTouchUV;
        private List<AndroidAppViewTouchEvent> m_CachedTouchEvents;
#endif

        protected AndroidAppView2D()
        {
#if ENABLE_ANDROID_APPVIEW
            m_IsTouchInsideView = false;
            m_Dragging = false;
            m_LastValidTouchUV = Vector2.zero;
            m_CachedTouchEvents = new List<AndroidAppViewTouchEvent>();
#endif
        }

        protected override void OnEnable()
        {
            base.OnEnable();
#if ENABLE_ANDROID_APPVIEW
            m_TouchInputPlane = transform as RectTransform;
            m_LastValidTouchUV = Vector2.zero;
    #if !UNITY_EDITOR
            m_AndroidAppViewController = AndroidAppViewController.NewObject(androidAppViewSettings);    // activate AndroidAppViewController when enabled
            if (androidAppViewSettings != null)
                AndroidAppViewManager.RegisterEventCallback(androidAppViewSettings.targetAppPackageName, this);
    #endif
#endif
        }

        protected override void OnDisable()
        {
            base.OnDisable();
#if !UNITY_EDITOR && ENABLE_ANDROID_APPVIEW
            if (androidAppViewSettings != null)
                AndroidAppViewManager.UnregisterEventCallback(androidAppViewSettings.targetAppPackageName, this);
#endif
        }

        public override Texture mainTexture
        {
            get
            {
                if (material != null && material.mainTexture != null)
                {
                    return material.mainTexture;
                }
                return s_WhiteTexture;
            }
        }

        protected override void OnDidApplyAnimationProperties()
        {
            SetMaterialDirty();
            SetVerticesDirty();
            SetRaycastDirty();
        }

#if UNITY_EDITOR || ENABLE_ANDROID_APPVIEW
        internal void ApplyAndroidAppViewSettings()
        {
            RenderTexture rt = androidAppViewSettings != null ? androidAppViewSettings.targetRenderTexture : null;
            if (material != null && material.mainTexture != rt)
            {
                material.mainTexture = rt;

                SetVerticesDirty();
                SetMaterialDirty();
            }
        }
#endif

#if UNITY_EDITOR || ENABLE_ANDROID_APPVIEW
        public AndroidAppViewSettings androidAppViewSettings
        {
            set
            {
                if (m_AndroidAppViewSettings == value)
                    return;
#if ENABLE_ANDROID_APPVIEW
                if (m_AndroidAppViewSettings != null && value != null)
                {
                    if (m_AndroidAppViewSettings.targetAppPackageName == value.targetAppPackageName)
                    {
                        Debug.LogWarning("AndroidAppView2D do not support change androidAppViewSettings with same targetAppPackageName!");
                        return;
                    }
                }
                if (m_AndroidAppViewSettings != null)
                    AndroidAppViewManager.UnregisterEventCallback(m_AndroidAppViewSettings.targetAppPackageName, this);
#endif

                m_AndroidAppViewSettings = value;
#if ENABLE_ANDROID_APPVIEW
                m_AndroidAppViewController = AndroidAppViewController.NewObject(androidAppViewSettings);    // set AndroidAppViewController when androidAppViewSettings changed
                ApplyAndroidAppViewSettings();
                if (m_AndroidAppViewSettings != null)
                    AndroidAppViewManager.RegisterEventCallback(m_AndroidAppViewSettings.targetAppPackageName, this);
#endif
            }
            get
            {
                return m_AndroidAppViewSettings;
            }
        }
#endif

#if ENABLE_ANDROID_APPVIEW
        public AndroidAppViewController androidAppViewController
        {
            get
            {
                if (m_AndroidAppViewController == null)
                    m_AndroidAppViewController = AndroidAppViewController.NewObject(androidAppViewSettings);
                return m_AndroidAppViewController;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_IsTouchInsideView = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_IsTouchInsideView = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (m_IsTouchInsideView && m_Dragging)
                ProcessInputEventInternal(TouchPhase.Moved, eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            m_Dragging = true;
            if (m_IsTouchInsideView)
                ProcessInputEventInternal(TouchPhase.Began, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            m_Dragging = false;
            if (m_IsTouchInsideView)
            {
                ProcessInputEventInternal(TouchPhase.Ended, eventData);
            }
            else if (m_LastValidTouchUV != Vector2.zero)
            {
                m_CachedTouchEvents.Add(new AndroidAppViewTouchEvent(m_LastValidTouchUV, TouchPhase.Ended));
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (m_IsTouchInsideView && !m_Dragging)
            {
                Vector2 outputTouchUV = ProcessInputEventInternal(TouchPhase.Began, eventData);
                if (outputTouchUV != Vector2.zero)
                    m_CachedTouchEvents.Add(new AndroidAppViewTouchEvent(outputTouchUV, TouchPhase.Ended));
            }
        }

        protected Vector2 ProcessInputEventInternal(TouchPhase phase, PointerEventData data)
        {
            if (data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
                m_TouchInputPlane = data.pointerEnter.transform as RectTransform;

            Vector2 localTouchPos;
            Vector2 outputTouchUV = Vector2.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_TouchInputPlane, data.position, data.pressEventCamera, out localTouchPos))
            {
                outputTouchUV.x = localTouchPos.x / m_TouchInputPlane.rect.width + 0.5f;
                outputTouchUV.y = 0.5f - (localTouchPos.y / m_TouchInputPlane.rect.height);     // flip y
                outputTouchUV.x = Mathf.Clamp(outputTouchUV.x, 0f, 1f);
                outputTouchUV.y = Mathf.Clamp(outputTouchUV.y, 0f, 1f);

                if (phase == TouchPhase.Began || phase == TouchPhase.Moved)
                    m_LastValidTouchUV = outputTouchUV;

                m_CachedTouchEvents.Add(new AndroidAppViewTouchEvent(outputTouchUV, phase));
            }
            return outputTouchUV;
        }

        public void OnRenderTextureAvailable() {}

        public List<AndroidAppViewTouchEvent> OnProcessTouchInputEvents()
        {
            List<AndroidAppViewTouchEvent> cachedTouchEvents = new List<AndroidAppViewTouchEvent>(m_CachedTouchEvents);
            m_CachedTouchEvents.Clear();
            return cachedTouchEvents;
        }
#endif
    }
}
