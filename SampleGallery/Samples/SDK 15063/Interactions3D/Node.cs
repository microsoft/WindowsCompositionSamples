using Microsoft.Graphics.Canvas.Text;
using System;
using System.Numerics;

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


        public ImageNodeInfo GenerateRandomImageNode(int maxCount)
        {
            m_nextImageIndex = (m_nextImageIndex + 1) % maxCount;

            m_nextPlaneIndex = (m_nextPlaneIndex + 1) % m_zPlanes.Length;

            var imageNode = new ImageNodeInfo(
                                        m_nextImageIndex,
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
        public ImageNodeInfo(int imageIndex, Vector3 offset, float scale, float opacity = 1.0f) :
            base(offset, scale, opacity)
        {
            ImageIndex = imageIndex;
        }

        public int ImageIndex
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
}
