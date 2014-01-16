using UnityEngine;

namespace TouchScript.Hit
{
    public struct TouchHit
    {

        public enum HitType
        {
            Hit3D,
            Hit2D
        }

        public static TouchHit FromRaycastHit(RaycastHit value)
        {
            var result = new TouchHit()
            {
                Type = HitType.Hit3D,
                BarycentricCoordinate = value.barycentricCoordinate,
                Collider = value.collider,
                Distance = value.distance,
                LightmapCoord = value.lightmapCoord,
                Normal = value.normal,
                Point = value.point,
                Rigidbody = value.rigidbody,
                TextureCoord = value.textureCoord,
                TextureCoord2 = value.textureCoord2,
                Transform = value.collider.transform,
                TriangleIndex = value.triangleIndex
            };
            return result;
        }

        public HitType Type;
        public Vector3 BarycentricCoordinate;
        public Collider Collider;
        public float Distance;
        public Vector2 LightmapCoord;
        public Vector3 Normal;
        public Vector3 Point;
        public Rigidbody Rigidbody;
        public Vector2 TextureCoord;
        public Vector2 TextureCoord2;
        public Transform Transform;
        public int TriangleIndex;
    }
}