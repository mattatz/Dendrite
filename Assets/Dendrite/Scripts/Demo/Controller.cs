using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Dendrite.Demo
{

    public class Controller : MonoBehaviour
    {

        [SerializeField] protected DendriteBase dendrite;
        [SerializeField] protected List<DendriteRenderingBase> renderings;

        #region MonoBehaviour

        protected void OnEnable()
        {
            dendrite = GameObject.FindObjectOfType<DendriteBase>();
            renderings = Resources.FindObjectsOfTypeAll<DendriteRenderingBase>().ToList();
            renderings.ForEach(r => r.Dendrite = dendrite);
        }

        protected void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                Reset();
            }
        }

        protected void OnGUI()
        {
            const float labelWidth = 60f, sliderWidth = 100f;

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(20f);
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Space(20f);

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("influence : ", GUILayout.Width(labelWidth));
                        dendrite.InfluenceDistance = GUILayout.HorizontalSlider(dendrite.InfluenceDistance, 0.25f, 3f, GUILayout.Width(sliderWidth));
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("growth : ", GUILayout.Width(labelWidth));
                        dendrite.GrowthDistance = GUILayout.HorizontalSlider(dendrite.GrowthDistance, 0.25f, 1f, GUILayout.Width(sliderWidth));
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("kill : ", GUILayout.Width(labelWidth));
                        dendrite.KillDistance = GUILayout.HorizontalSlider(dendrite.KillDistance, 0.25f, 1f, GUILayout.Width(sliderWidth));
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("speed : ", GUILayout.Width(labelWidth));
                        dendrite.GrowthSpeed = GUILayout.HorizontalSlider(dendrite.GrowthSpeed, 0.25f, 300f, GUILayout.Width(sliderWidth));
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        if(GUILayout.Button("Reset"))
                        {
                            Reset();
                        }
                    }
                }
            }
        }

        #endregion

        protected void Reset()
        {

            if (dendrite != null) dendrite.Reset();
            renderings.ForEach(r => r.Clear());
        }

    }

}


