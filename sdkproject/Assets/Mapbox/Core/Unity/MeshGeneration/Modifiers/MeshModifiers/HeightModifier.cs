namespace Mapbox.Unity.MeshGeneration.Modifiers
{
    using System.Collections.Generic;
    using UnityEngine;
    using Mapbox.Unity.MeshGeneration.Data;

    public enum ExtrusionType
    {
        Wall,
        FirstMidFloor,
        FirstMidTopFloor
    }

    [CreateAssetMenu(menuName = "Mapbox/Modifiers/Height Modifier")]
    public class HeightModifier : MeshModifier
    {
        [SerializeField]
        private bool _flatTops;
        [SerializeField]
        private float _height;
        [SerializeField]
        private bool _forceHeight;

        public override ModifierType Type { get { return ModifierType.Preprocess; } }

        public override void Run(VectorFeatureUnity feature, MeshData md, UnityTile tile = null)
        {
            if (md.Vertices.Count == 0 || feature == null || feature.Points.Count < 1)
                return;

            float hf = _height;
            if (!_forceHeight)
            {
                if (feature.Properties.ContainsKey("height"))
                {
                    if (float.TryParse(feature.Properties["height"].ToString(), out hf))
                    {
                        if (feature.Properties.ContainsKey("min_height"))
                        {
                            hf -= float.Parse(feature.Properties["min_height"].ToString());
                        }
                    }
                }
                if (feature.Properties.ContainsKey("ele"))
                {
                    if (float.TryParse(feature.Properties["ele"].ToString(), out hf))
                    {
                    }
                }
            }

            var max = md.Vertices[0].y;
            var min = md.Vertices[0].y;
            if (_flatTops)
            {
                for (int i = 0; i < md.Vertices.Count; i++)
                {
                    if (md.Vertices[i].y > max)
                        max = md.Vertices[i].y;
                    else if (md.Vertices[i].y < min)
                        min = md.Vertices[i].y;
                }
                for (int i = 0; i < md.Vertices.Count; i++)
                {
                    md.Vertices[i] = new Vector3(md.Vertices[i].x, max + hf, md.Vertices[i].z);
                }
                hf += max - min;
            }
            else
            {
                for (int i = 0; i < md.Vertices.Count; i++)
                {
                    md.Vertices[i] = new Vector3(md.Vertices[i].x, md.Vertices[i].y + hf, md.Vertices[i].z);
                }
            }           

            var vertsStartCount = 0;
            var count = md.Vertices.Count;
            float d = 0f;
            Vector3 v1;
            Vector3 v2;
            int ind = 0;

            var wallTri = new List<int>();
            var wallUv = new List<Vector2>();
            for (int i = 0; i < feature.Points.Count; i++)
            {
                var length = feature.Points[i].Count;
                for (int j = 1; j < 2*length; j++)
                {
                    v1 = md.Vertices[vertsStartCount + j - 1];
                    v2 = md.Vertices[vertsStartCount + j];
                    ind = md.Vertices.Count;
                    md.Vertices.Add(v1);
                    md.Vertices.Add(v2);
                    md.Vertices.Add(new Vector3(v1.x, md.Vertices[j - 1].y - hf, v1.z));
                    md.Vertices.Add(new Vector3(v2.x, md.Vertices[j].y - hf, v2.z));

                    d = (v2 - v1).magnitude;

                    wallUv.Add(new Vector2(0, 0));
                    wallUv.Add(new Vector2(d, 0));
                    wallUv.Add(new Vector2(0, -hf));
                    wallUv.Add(new Vector2(d, -hf));

                    wallTri.Add(ind);
                    wallTri.Add(ind + 2);
                    wallTri.Add(ind + 1);

                    wallTri.Add(ind + 1);
                    wallTri.Add(ind + 2);
                    wallTri.Add(ind + 3);
                }

                //v1 = md.Vertices[vertsStartCount];
                //v2 = md.Vertices[vertsStartCount + length - 1];
                //ind = md.Vertices.Count;
                //md.Vertices.Add(v1);
                //md.Vertices.Add(v2);
                //md.Vertices.Add(new Vector3(v1.x, md.Vertices[ind].y - hf, v1.z));
                //md.Vertices.Add(new Vector3(v2.x, md.Vertices[ind].y - hf, v2.z));

                //d = (v2 - v1).magnitude;

                //wallUv.Add(new Vector2(0, 0));
                //wallUv.Add(new Vector2(d, 0));
                //wallUv.Add(new Vector2(0, -hf));
                //wallUv.Add(new Vector2(d, -hf));

                //wallTri.Add(ind);
                //wallTri.Add(ind + 1);
                //wallTri.Add(ind + 2);

                //wallTri.Add(ind + 1);
                //wallTri.Add(ind + 3);
                //wallTri.Add(ind + 2);

                vertsStartCount += 2*length;
            }
            md.Triangles.Add(wallTri);
            md.UV[0].AddRange(wallUv);

        }
    }
}
