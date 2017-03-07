using System;
using System.Numerics;
using Microsoft.Graphics.Canvas.Text;

namespace CompositionSampleGallery
{
    internal class NodeManager
    {
        public static NodeManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new NodeManager();
                }

                return s_instance;
            }
        }


        private NodeManager()
        {
            MinPosition = new Vector3(-2500, -380, -1400);
            MaxPosition = new Vector3( 2500,  290,    10);

            MinScale = 0.08f;
            MaxScale = 0.40f;
        }


        public ImageNodeInfo GenerateRandomImageNode()
        {
            m_nextImageIndex = (m_nextImageIndex + 1) % (int)NamedImage.Count;

            m_nextPlaneIndex = (m_nextPlaneIndex + 1) % m_zPlanes.Length;

            var imageNode = new ImageNodeInfo(
                                        (NamedImage)m_nextImageIndex,
                                        new Vector3(
                                            m_random.Next((int)(MinPosition.X * 100), (int)(MaxPosition.X * 100)) * 0.01f,
                                            m_random.Next((int)(MinPosition.Y * 100), (int)(MaxPosition.Y * 100)) * 0.01f,
                                            m_random.Next((int)(MinPosition.Z * 100), (int)(MaxPosition.Z * 100)) * 0.01f + m_zPlanes[m_nextPlaneIndex]),
                                        m_random.Next((int)(MinScale * 100), (int)(MaxScale * 100)) * 0.01f);
                        
            return imageNode;
        }

        public Vector3 MinPosition
        {
            get; set;
        }


        public Vector3 MaxPosition
        {
            get; set;
        }

        public float MinScale
        {
            get; set;
        }

        public float MaxScale
        {
            get; set;
        }


        private static NodeManager s_instance;

        private int m_nextImageIndex = -1;
        private int m_nextPlaneIndex = -1;
        private Random m_random = new Random();

        private float[] m_zPlanes = new float[] { 0, -200, -600, -1200, -1800 };
    }


    internal abstract class NodeInfo : IComparable<NodeInfo>
    {
        protected NodeInfo(Vector3 offset, float scale, float opacity)
        {
            Offset = offset;
            Scale = scale;
            Opacity = opacity;
        }

        public Vector3 Offset
        {
            get; set;
        }

        public float Opacity
        {
            get; set;
        }

        public float Scale
        {
            get
            {
                return m_scale;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                m_scale = value;
            }
        }

        private float m_scale = 1.0f;

        public int CompareTo(NodeInfo other)
        {
            if (this.Offset.Z < other.Offset.Z)
            {
                return -1;
            }
            else if (this.Offset.Z > other.Offset.Z)
            {
                return 1;
            }
            else
            {
                if (this.Offset.X < other.Offset.X)
                {
                    return -1;
                }
                else if (this.Offset.X > other.Offset.X)
                {
                    return 1;
                }
                else
                {
                    if (this.Offset.Y < other.Offset.Y)
                    {
                        return -1;
                    }
                    else if (this.Offset.Y > other.Offset.Y)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }
    }



    internal class ImageNodeInfo : NodeInfo
    {
        public ImageNodeInfo(NamedImage namedImage, Vector3 offset, float scale, float opacity = 1.0f) :
            base(offset, scale, opacity)
        {
            NamedImage = namedImage;
        }

        public NamedImage NamedImage
        {
            get; set;
        }
    }



    internal class TextNodeInfo : NodeInfo
    {
        public TextNodeInfo(String text, CanvasTextFormat textFormat, Vector2 textureSize, Vector3 offset, float scale, float opacity, bool applyDistanceEffects) : 
            base(offset, scale, opacity)
        {
            Text = text;
            TextFormat = textFormat;
            TextureSize = textureSize;
            ApplyDistanceEffects = applyDistanceEffects;
        }

        public string Text
        {
            get; set;
        }

        public bool ApplyDistanceEffects
        {
            get; set;
        }

        public CanvasTextFormat TextFormat
        {
            get; set;
        }

        public Vector2 TextureSize
        {
            get; set;
        }

        public int BrushIndex
        {
            get; set;
        }
    }


    internal enum NamedImage
    {
        Pic00,
        Pic01,
        Pic02,
        Pic03,
        Pic04,
        Pic05,
        Pic06,
        Pic07,
        Pic08,
        Pic09,
        Pic10,
        Pic11,
        Pic12,
        Pic13,
        Pic14,
        Pic15,
        Pic16,
        Pic17,
        Pic18,
        Pic19,
        Pic20,
        Pic21,
        Pic22,
        Pic23,
        Pic24,
        Pic25,
        Pic26,
        Pic27,
        Pic28,
        Pic29,
        Pic30,
        Pic31,
        Pic32,
        Pic33,
        Pic34,
        Pic35,
        Pic36,
        Pic37,
        Pic38,
        Pic39,
        Pic40,
        Pic41,
        Pic42,
        Pic43,
        Pic44,
        Pic45,

        Count,
    }
}
