using UnityEngine;

public static class Extensions
{
    private static LayerMask layerMask = LayerMask.GetMask("Default");

    public static bool CircleCast(this Rigidbody2D rigidbody, Vector2 direction, float distance = 0.375f, float radius = 0.25f)
    {
        if (rigidbody.bodyType == RigidbodyType2D.Kinematic)
        {
            return false;
        }

        RaycastHit2D hit = Physics2D.CircleCast(rigidbody.position, radius, direction.normalized, distance, layerMask);
        return hit.collider != null && hit.rigidbody != rigidbody;
    }

    public static GameObject CircleCastObject(this Rigidbody2D rigidbody, Vector2 direction, float distance = 0.375f, float radius = 0.25f) {

        RaycastHit2D hit = Physics2D.CircleCast(rigidbody.position, radius, direction.normalized, distance, layerMask);
        return hit.collider.gameObject;
    }


    public static bool BoxCast(this Rigidbody2D rigidbody, Vector2 size, Vector2 offset)
    {
        if (rigidbody.bodyType == RigidbodyType2D.Kinematic)
        {
            return false;
        }

        Vector2 origin = new Vector2(rigidbody.position.x + offset.x, rigidbody.position.y + offset.y);
        float timcheese = rigidbody.position.x + offset.x;

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0, Vector2.down, layerMask);
        return hit.collider != null && hit.rigidbody != rigidbody;
    }
}
