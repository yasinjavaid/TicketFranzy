using MoreLinq;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Bounds = UnityEngine.Bounds;

public static class UnityEngineExtensions
{
    #region GetChild
    public static GameObject GetChild(this GameObject gameObject, int index)
        => gameObject.transform.GetChild(index).gameObject;

    public static GameObject GetChildObject(this GameObject gameObject, int index)
        => gameObject.transform.GetChild(index).gameObject;

    public static GameObject GetChildObject(this Transform transform, int index)
        => transform.GetChild(index).gameObject;

    public static Transform GetChildTransform(this GameObject gameObject, int index)
        => gameObject.transform.GetChild(index);

    public static Transform GetChildTransform(this Transform transform, int index)
        => transform.GetChild(index);

    public static Transform FindDeepChild(this Component component, string name)
        => component.transform.FindDeepChild(name);

    public static GameObject FindDeepChild(this GameObject gameObject, string name)
        => gameObject.transform.FindDeepChild(name).gameObject;

    public static Transform FindDeepChild(this Transform transform, string name)
    {
        if (name.Contains('/'))
        {
            Transform child = transform;
            foreach (string childName in name.Split('/'))
            {
                child = child.Find(childName);
                if (!child) break;
            }
            if (child) return child;
        }

        Transform t = transform.Find(name);
        if (t) return t;

        for (int i = 0; i < transform.childCount; i++)
        {
            t = FindDeepChild(transform.GetChild(i), name);
            if (t) return t;
        }
        return null;
    }

    public static Transform FindInactiveChild(this Transform transform, string name)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).name == name)
                return transform.GetChild(i);
        }
        return null;
    }

    public static bool TryGetChild(this Transform transform, int index, out Transform child)
    {
        if (transform && index >= 0 && index < transform.childCount)
        {
            child = transform.GetChild(index);
            return true;
        }
        else
        {
            child = default;
            return false;
        }
    }

    public static bool TryFind(this Transform transform, string name, out Transform child) => child = transform.Find(name);

    public static bool TryFindDeepChild(this Transform transform, string name, out Transform child) => child = transform.FindDeepChild(name);
    #endregion

    #region GetOrAddComponent
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        => gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();

    public static T GetOrAddComponent<T>(this Transform t) where T : Component
        => GetOrAddComponent<T>(t.gameObject);

    public static T GetOrAddComponent<T>(this Component c) where T : Component
        => GetOrAddComponent<T>(c.gameObject);
    #endregion

    #region HasComponent
    public static bool HasComponent<T>(this GameObject gameObject) where T : Component
        => gameObject.GetComponent<T>() != null;

    public static bool HasComponent<T>(this Transform transform) where T : Component
        => transform.GetComponent<T>() != null;

    public static bool HasComponent<T>(this Component component) where T : Component
        => component.GetComponent<T>() != null;

    public static bool HasComponentInParent<T>(this GameObject gameObject) where T : Component
        => gameObject.GetComponentInParent<T>() != null;

    public static bool HasComponentInParent<T>(this Transform transform) where T : Component
        => transform.GetComponentInParent<T>() != null;

    public static bool HasComponentInParent<T>(this Component component) where T : Component
        => component.GetComponentInParent<T>() != null;

    public static bool HasComponentInChildren<T>(this GameObject gameObject) where T : Component
        => gameObject.GetComponentInChildren<T>() != null;

    public static bool HasComponentInChildren<T>(this Transform transform) where T : Component
        => transform.GetComponentInChildren<T>() != null;

    public static bool HasComponentInChildren<T>(this Component component) where T : Component
        => component.GetComponentInChildren<T>() != null;

    public static bool TryGetComponent<T>(this GameObject gameObject, out T t) where T : Component
    {
        if (gameObject == null) { t = null; return false; }
        t = gameObject.GetComponent<T>();
        return t != null;
    }

    public static bool TryGetComponent<T>(this Transform transform, out T t) where T : Component
    {
        if (transform == null) { t = null; return false; }
        t = transform.GetComponent<T>();
        return t != null;
    }

    public static bool TryGetComponent<T>(this Component component, out T t) where T : Component
    {
        if (component == null) { t = null; return false; }
        t = component.GetComponent<T>();
        return t != null;
    }

    public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T t) where T : Component
    {
        if (gameObject == null) { t = null; return false; }
        t = gameObject.GetComponentInParent<T>();
        return t != null;
    }

    public static bool TryGetComponentInParent<T>(this Transform transform, out T t) where T : Component
    {
        if (transform == null) { t = null; return false; }
        t = transform.GetComponentInParent<T>();
        return t != null;
    }

    public static bool TryGetComponentInParent<T>(this Component component, out T t) where T : Component
    {
        if (component == null) { t = null; return false; }
        t = component.GetComponentInParent<T>();
        return t != null;
    }

    public static bool TryGetComponentInChildren<T>(this GameObject gameObject, out T t) where T : Component
    {
        if (gameObject == null) { t = null; return false; }
        t = gameObject.GetComponentInChildren<T>();
        return t != null;
    }

    public static bool TryGetComponentInChildren<T>(this Transform transform, out T t) where T : Component
    {
        if (transform == null) { t = null; return false; }
        t = transform.GetComponentInChildren<T>();
        return t != null;
    }

    public static bool TryGetComponentInChildren<T>(this Component component, out T t) where T : Component
    {
        if (component == null) { t = null; return false; }
        t = component.GetComponentInChildren<T>();
        return t != null;
    }

    public static bool TryGetComponentInParentOrChildren<T>(this GameObject gameObject, out T t) where T : Component
        => TryGetComponentInParent(gameObject, out t) || TryGetComponentInChildren(gameObject, out t);

    public static bool TryGetComponentInParentOrChildren<T>(this Transform transform, out T t) where T : Component
        => TryGetComponentInParent(transform, out t) || TryGetComponentInChildren(transform, out t);

    public static bool TryGetComponentInParentOrChildren<T>(this Component component, out T t) where T : Component
        => TryGetComponentInParent(component, out t) || TryGetComponentInChildren(component, out t);
    #endregion

    #region Vector

    public static Vector2 Squared(this Vector2 v) => v.normalized * v.sqrMagnitude;

    public static Vector3 Squared(this Vector3 v) => v.normalized * v.sqrMagnitude;

    public static Vector2 Add(this Vector2 v, float f)
    {
        v.x += f;
        v.y += f;
        return v;
    }

    public static Vector2Int RoundToInt(this Vector2 vector) => new Vector2Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));

    /// <summary>
    /// Returns a vector composed of the Cos and the Sin of an angle in the form (Cos, Sin)
    /// </summary>
    /// <param name="angleDeg">an angle in degrees</param>
    public static Vector2 DegToVector(this float angleDeg) => RadToVector(angleDeg * Mathf.Deg2Rad);

    /// <summary>
    /// Returns a vector composed of the Cos and the Sin of an angle in the form (Cos, Sin)
    /// </summary>
    /// <param name="angleDeg">an angle in degrees</param>
    public static Vector2 DegToVector(this float angleDeg, float magnitude) => RadToVector(angleDeg * Mathf.Deg2Rad, magnitude);

    /// <summary>
    /// Returns a vector composed of the Cos and the Sin of an angle in the form (Cos, Sin)
    /// </summary>
    /// <param name="angleRad">an angle in radians</param>
    public static Vector2 RadToVector(this float angleRad) => new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

    /// <summary>
    /// Returns a vector composed of the Cos and the Sin of an angle in the form (Cos, Sin)
    /// </summary>
    /// <param name="angleRad">an angle in radians</param>
    public static Vector2 RadToVector(this float angleRad, float magnitude) => new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * magnitude;

    /// <summary>
    /// Returns the angle in radians whose Tan is y/x.
    /// </summary>
    /// <returns>The angle between the x-axis and a 2D vector starting at zero and terminating at(x, y)</returns>
    public static float VectorToRad(this Vector2 vector) => Mathf.Atan2(vector.y, vector.x);

    /// <summary>
    /// Returns the angle in radians whose Tan is y/x.
    /// </summary>
    /// <returns>The angle between the x-axis and a 2D vector starting at zero and terminating at(x, y)</returns>
    public static float GetAngleRad(this Vector2 vector) => Mathf.Atan2(vector.y, vector.x);

    /// <summary>
    /// Returns the angle in degrees whose Tan is y/x.
    /// </summary>
    /// <returns>The angle between the x-axis and a 2D vector starting at zero and terminating at(x, y)</returns>
    public static float VectorToDeg(this Vector2 vector) => VectorToRad(vector) * Mathf.Rad2Deg;

    /// <summary>
    /// Returns the angle in degrees whose Tan is y/x.
    /// </summary>
    /// <returns>The angle between the x-axis and a 2D vector starting at zero and terminating at(x, y)</returns>
    public static float GetAngleDeg(this Vector2 vector) => VectorToRad(vector) * Mathf.Rad2Deg;

    /// <summary>
    /// Returns the angle in radians whose Tan is y/x.
    /// </summary>
    /// <returns>The angle between the x-axis and a 2D vector starting at zero and terminating at(x, y)</returns>
    public static float VectorToRad(this Vector2Int vector) => Mathf.Atan2(vector.y, vector.x);

    /// <summary>
    /// Returns the angle in radians whose Tan is y/x.
    /// </summary>
    /// <returns>The angle between the x-axis and a 2D vector starting at zero and terminating at(x, y)</returns>
    public static float GetAngleRad(this Vector2Int vector) => Mathf.Atan2(vector.y, vector.x);

    /// <summary>
    /// Returns the angle in degrees whose Tan is y/x.
    /// </summary>
    /// <returns>The angle between the x-axis and a 2D vector starting at zero and terminating at(x, y)</returns>
    public static float VectorToDeg(this Vector2Int vector) => VectorToRad(vector) * Mathf.Rad2Deg;

    /// <summary>
    /// Returns the angle in degrees whose Tan is y/x.
    /// </summary>
    /// <returns>The angle between the x-axis and a 2D vector starting at zero and terminating at(x, y)</returns>
    public static float GetAngleDeg(this Vector2Int vector) => VectorToRad(vector) * Mathf.Rad2Deg;

    /// <summary>
    /// Rotates a vector by a certain angle
    /// </summary>
    /// <param name="vector">The vector to be rotated</param>
    /// <param name="rad">The angle in radians</param>
    /// <returns>The resulting rotated vector</returns>
    public static Vector2 RotateRadians(this Vector2 vector, float rad) => vector == default ? vector : (vector.VectorToRad() + rad).RadToVector(vector.magnitude);

    /// <summary>
    /// Rotates a vector by a certain angle
    /// </summary>
    /// <param name="vector">The vector to be rotated</param>
    /// <param name="rad">The angle in radians</param>
    /// <returns>The resulting rotated vector, rounded back to int</returns>
    public static Vector2Int RotateRadians(this Vector2Int vector, float rad) => vector == default ? vector : (vector.VectorToRad() + rad).RadToVector(vector.magnitude).RoundToInt();

    /// <summary>
    /// Rotates a vector by a certain angle
    /// </summary>
    /// <param name="vector">The vector to be rotated</param>
    /// <param name="deg">The angle in degrees</param>
    /// <returns>The resulting rotated vector</returns>
    public static Vector2 RotateDegrees(this Vector2 vector, float deg) => RotateRadians(vector, deg * Mathf.Deg2Rad);

    /// <summary>
    /// Rotates a vector by a certain angle
    /// </summary>
    /// <param name="vector">The vector to be rotated</param>
    /// <param name="deg">The angle in degrees</param>
    /// <returns>The resulting rotated vector, rounded back to int</returns>
    public static Vector2Int RotateDegrees(this Vector2Int vector, float deg) => RotateRadians(vector, deg * Mathf.Deg2Rad);

    public static Vector2 Set(this Vector2 v, float? x = null, float? y = null)
    {
        if (x.HasValue)
            v.x = x.Value;
        if (y.HasValue)
            v.y = y.Value;
        return v;
    }

    public static Vector3 Set(this Vector3 v, float? x = null, float? y = null, float? z = null)
    {
        if (x.HasValue)
            v.x = x.Value;
        if (y.HasValue)
            v.y = y.Value;
        if (z.HasValue)
            v.z = z.Value;
        return v;
    }

    public static Vector3 ToVector3_XZ(this Vector2 v) => new Vector3(v.x, 0, v.y);

    public static void Update(this ref Vector2 v, float? x = null, float? y = null)
    {
        if (x.HasValue)
            v.x = x.Value;
        if (y.HasValue)
            v.y = y.Value;
    }

    public static void Update(this ref Vector3 v, float? x = null, float? y = null, float? z = null)
    {
        if (x.HasValue)
            v.x = x.Value;
        if (y.HasValue)
            v.y = y.Value;
        if (z.HasValue)
            v.z = z.Value;
    }

    /// <summary>
    /// Multiplies each passed parameter with their corresponding place in the vector.
    /// Example: if v = (2, 1) and x = 3, return will be (6, 1)
    /// </summary>
    /// <param name="v">The original vector to be multiplied</param>
    /// <param name="x">The factor by which to multiply the x component to the vector</param>
    /// <param name="y">The factor by which to multiply the y component to the vector</param>
    /// <returns></returns>
    public static Vector2 MultiplyIndividually(this Vector2 v, float? x = null, float? y = null)
    {
        if (x.HasValue)
            v.x *= x.Value;
        if (y.HasValue)
            v.y *= y.Value;
        return v;
    }

    /// <summary>
    /// Multiplies each passed parameter with their corresponding place in the vector.
    /// Example: if v = (2, 1, 1) and x = 3, return will be (6, 1, 1)
    /// </summary>
    /// <param name="v">The original vector to be multiplied</param>
    /// <param name="x">The factor by which to multiply the x component to the vector</param>
    /// <param name="y">The factor by which to multiply the y component to the vector</param>
    /// <param name="z">The factor by which to multiply the z component to the vector</param>
    /// <returns></returns>
    public static Vector3 MultiplyIndividually(this Vector3 v, float? x = null, float? y = null, float? z = null)
    {
        if (x.HasValue)
            v.x *= x.Value;
        if (y.HasValue)
            v.y *= y.Value;
        if (z.HasValue)
            v.z *= z.Value;
        return v;
    }

    /// <summary>
    /// Multiplies each passed parameter with their corresponding place in the vector.
    /// Example: if v = (2, 1) and x = 3, return will be (6, 1)
    /// </summary>
    /// <param name="v">The original vector to be multiplied</param>
    /// <param name="x">The factor by which to multiply the x component to the vector</param>
    /// <param name="y">The factor by which to multiply the y component to the vector</param>
    /// <returns></returns>
    public static Vector2 MultiplyIndividually(this Vector2 v, Vector2 other)
    {
        v.x *= other.x;
        v.y *= other.y;
        return v;
    }

    /// <summary>
    /// Multiplies each passed parameter with their corresponding place in the vector.
    /// Example: if v = (2, 1, 1) and x = 3, return will be (6, 1, 1)
    /// </summary>
    /// <param name="v">The original vector to be multiplied</param>
    /// <param name="x">The factor by which to multiply the x component to the vector</param>
    /// <param name="y">The factor by which to multiply the y component to the vector</param>
    /// <param name="z">The factor by which to multiply the z component to the vector</param>
    /// <returns></returns>
    public static Vector3 MultiplyIndividually(this Vector3 v, Vector3 other)
    {
        v.x *= other.x;
        v.y *= other.y;
        v.z *= other.z;
        return v;
    }

    /// <summary>
    /// Multiplies each passed parameter with their corresponding place in the vector.
    /// Example: if v = (2, 1) and x = 3, return will be (6, 1)
    /// </summary>
    /// <param name="v">The original vector to be multiplied</param>
    /// <param name="x">The factor by which to multiply the x component to the vector</param>
    /// <param name="y">The factor by which to multiply the y component to the vector</param>
    /// <returns></returns>
    public static Vector2 DivideIndividually(this Vector2 v, float? x = null, float? y = null)
    {
        if (x.HasValue)
            v.x /= x.Value;
        if (y.HasValue)
            v.y /= y.Value;
        return v;
    }

    /// <summary>
    /// Multiplies each passed parameter with their corresponding place in the vector.
    /// Example: if v = (2, 1, 1) and x = 3, return will be (6, 1, 1)
    /// </summary>
    /// <param name="v">The original vector to be multiplied</param>
    /// <param name="x">The factor by which to multiply the x component to the vector</param>
    /// <param name="y">The factor by which to multiply the y component to the vector</param>
    /// <param name="z">The factor by which to multiply the z component to the vector</param>
    /// <returns></returns>
    public static Vector3 DivideIndividually(this Vector3 v, float? x = null, float? y = null, float? z = null)
    {
        if (x.HasValue)
            v.x /= x.Value;
        if (y.HasValue)
            v.y /= y.Value;
        if (z.HasValue)
            v.z /= z.Value;
        return v;
    }

    /// <summary>
    /// Multiplies each passed parameter with their corresponding place in the vector.
    /// Example: if v = (2, 1) and x = 3, return will be (6, 1)
    /// </summary>
    /// <param name="v">The original vector to be multiplied</param>
    /// <param name="x">The factor by which to multiply the x component to the vector</param>
    /// <param name="y">The factor by which to multiply the y component to the vector</param>
    /// <returns></returns>
    public static Vector2 DivideIndividually(this Vector2 v, Vector2 other)
    {
        v.x /= other.x;
        v.y /= other.y;
        return v;
    }

    /// <summary>
    /// Multiplies each passed parameter with their corresponding place in the vector.
    /// Example: if v = (2, 1, 1) and x = 3, return will be (6, 1, 1)
    /// </summary>
    /// <param name="v">The original vector to be multiplied</param>
    /// <param name="x">The factor by which to multiply the x component to the vector</param>
    /// <param name="y">The factor by which to multiply the y component to the vector</param>
    /// <param name="z">The factor by which to multiply the z component to the vector</param>
    /// <returns></returns>
    public static Vector3 DivideIndividually(this Vector3 v, Vector3 other)
    {
        v.x /= other.x;
        v.y /= other.y;
        v.z /= other.z;
        return v;
    }

    /// <summary>
    /// Sets the sign of each component in a vector individually, by getting the magnitudes from the vector and the signs from the parameters
    /// Example: if v = (-2, 2) and x = -2 and y = -1, return will be (-2, -2)
    /// </summary>
    /// <param name="v">The original vector to be updated</param>
    /// <param name="x">The new sign the x component shall have</param>
    /// <param name="y">The new sign the y component shall have</param>
    /// <returns>The vector with the changed components</returns>
    /// <seealso cref="UpdateSign(ref Vector2, float?, float?)"/>
    public static Vector2 SetSign(this Vector2 v, float? x = null, float? y = null)
    {
        if (x.HasValue)
            v.x = Mathf.Abs(v.x) * Mathf.Sign(x.Value);
        if (y.HasValue)
            v.y = Mathf.Abs(v.y) * Mathf.Sign(y.Value);
        return v;
    }

    /// <summary>
    /// Sets the sign of each component in a vector individually, by getting the magnitudes from the vector and the signs from the parameters
    /// Example: if v = (-2, 2, 1) and x = -2 and y = -1, return will be (-2, -2, 1)
    /// </summary>
    /// <param name="v">The original vector to be updated</param>
    /// <param name="x">The new sign the x component shall have</param>
    /// <param name="y">The new sign the y component shall have</param>
    /// <param name="z">The new sign the z component shall have</param>
    /// <returns>The vector with the changed components</returns>
    /// <seealso cref="UpdateSign(ref Vector3, float?, float?, float?)"/>
    public static Vector3 SetSign(this Vector3 v, float? x = null, float? y = null, float? z = null)
    {
        if (x.HasValue)
            v.x = Mathf.Abs(v.x) * Mathf.Sign(x.Value);
        if (y.HasValue)
            v.y = Mathf.Abs(v.y) * Mathf.Sign(y.Value);
        if (z.HasValue)
            v.z = Mathf.Abs(v.z) * Mathf.Sign(z.Value);
        return v;
    }


    /// <summary>
    /// Updates the sign of each component in a vector individually, by getting the magnitudes from the vector and the signs from the parameters
    /// Example: if v = (-2, 2) and x = -2 and y = -1, return will be (-2, -2)
    /// </summary>
    /// <param name="v">The original vector to be updated</param>
    /// <param name="x">The new sign the x component shall have</param>
    /// <param name="y">The new sign the y component shall have</param>
    /// <seealso cref="SetSign(Vector2, float?, float?)"/>
    public static void UpdateSign(this ref Vector2 v, float? x = null, float? y = null)
    {
        if (x.HasValue)
            v.x = Mathf.Abs(v.x) * Mathf.Sign(x.Value);
        if (y.HasValue)
            v.y = Mathf.Abs(v.y) * Mathf.Sign(y.Value);
    }

    /// <summary>
    /// Updates the sign of each component in a vector individually, by getting the magnitudes from the vector and the signs from the parameters
    /// Example: if v = (-2, 2, 1) and x = -2 and y = -1, return will be (-2, -2, 1)
    /// </summary>
    /// <param name="v">The original vector to be updated</param>
    /// <param name="x">The new sign the x component shall have</param>
    /// <param name="y">The new sign the y component shall have</param>
    /// <param name="z">The new sign the z component shall have</param>
    /// <seealso cref="SetSign(Vector3, float?, float?, float?)"/>
    public static void UpdateSign(this ref Vector3 v, float? x = null, float? y = null, float? z = null)
    {
        if (x.HasValue)
            v.x = Mathf.Abs(v.x) * Mathf.Sign(x.Value);
        if (y.HasValue)
            v.y = Mathf.Abs(v.y) * Mathf.Sign(y.Value);
        if (z.HasValue)
            v.z = Mathf.Abs(v.z) * Mathf.Sign(z.Value);
    }

    public static Vector2 RotateClockwise(this Vector2 v) => new Vector2(v.y, -v.x);
    public static Vector2 RotateCounterClockwise(this Vector2 v) => new Vector2(-v.y, v.x);

    /// <summary>
    /// EasyUnity: Rotates the vector by a certain amount of degrees
    /// </summary>
    /// <param name="v">The original vector to be rotated (not affected by this)</param>
    /// <param name="degrees">The angle in degrees to rotate the vector</param>
    /// <returns>The resulting rotated vector</returns>
    public static Vector2 Rotate(this Vector2 v, float degrees) => Quaternion.Euler(0, 0, degrees) * v;

    public static Vector2 Clamp(this Vector2 v, Rect rect)
    {
        v.x = Mathf.Clamp(v.x, rect.xMin, rect.xMax);
        v.y = Mathf.Clamp(v.y, rect.yMin, rect.yMax);
        return v;
    }

    public static Vector2 Clamp(this Vector2 v, Bounds bounds)
    {
        v.x = Mathf.Clamp(v.x, bounds.min.x, bounds.max.x);
        v.y = Mathf.Clamp(v.y, bounds.min.y, bounds.max.y);
        return v;
    }

    public static Vector3 Clamp(this Vector3 v, Bounds bounds)
    {
        v.x = Mathf.Clamp(v.x, bounds.min.x, bounds.max.x);
        v.y = Mathf.Clamp(v.y, bounds.min.y, bounds.max.y);
        v.z = Mathf.Clamp(v.z, bounds.min.z, bounds.max.z);
        return v;
    }

    public static Vector3 Clamp(this Vector3 v, FloatRange xRange, FloatRange yRange, FloatRange zRange)
    {
        v.x = Mathf.Clamp(v.x, xRange.Min, xRange.Max);
        v.y = Mathf.Clamp(v.y, yRange.Min, yRange.Max);
        v.z = Mathf.Clamp(v.z, zRange.Min, zRange.Max);
        return v;
    }

    public static Vector3Int Sum<T>(this IEnumerable<T> enumerable, Func<T, Vector3Int> selector)
    {
        Vector3Int sum = Vector3Int.zero;
        foreach (T t in enumerable)
            sum += selector(t);
        return sum;
    }

    //{
    //float sin = Mathf.Sin((float)degrees * Mathf.Deg2Rad);
    //float cos = Mathf.Cos((float)degrees * Mathf.Deg2Rad);

    //(v.x, v.y) = ((cos * v.x) - (sin * v.y), (sin * v.x) + (cos * v.y));
    //return v;
    //}

    #endregion

    #region Transform

    public static void UpdatePosition(this Transform t, float? x = null, float? y = null, float? z = null) => t.position = t.position.Set(x, y, z);
    public static void UpdateLocalPosition(this Transform t, float? x = null, float? y = null, float? z = null) => t.localPosition = t.localPosition.Set(x, y, z);

    public static void UpdatePosition(this GameObject go, float? x = null, float? y = null, float? z = null) => go?.transform.UpdatePosition(z, y, z);
    public static void UpdatePosition(this Component c, float? x = null, float? y = null, float? z = null) => c?.transform.UpdatePosition(z, y, z);
    public static void UpdateLocalPosition(this GameObject go, float? x = null, float? y = null, float? z = null) => go?.transform.UpdateLocalPosition(z, y, z);
    public static void UpdateLocalPosition(this Component c, float? x = null, float? y = null, float? z = null) => c?.transform.UpdateLocalPosition(z, y, z);

    public static RectTransform GetRectTransform(this GameObject go) => go?.transform as RectTransform;
    public static RectTransform GetRectTransform(this Component c) => c?.transform as RectTransform;
    public static RectTransform GetRectTransform(this Transform t) => t as RectTransform;

    public static float GetWidth(this RectTransform rt) => rt?.rect.width ?? 0;
    public static float GetWidth(this GameObject go) => GetRectTransform(go)?.rect.width ?? 0;
    public static float GetWidth(this Component c) => GetRectTransform(c)?.rect.width ?? 0;
    public static float GetWidth(this Transform t) => GetRectTransform(t)?.rect.width ?? 0;
    public static float GetHeight(this RectTransform rt) => rt?.rect.height ?? 0;
    public static float GetHeight(this GameObject go) => GetRectTransform(go)?.rect.height ?? 0;
    public static float GetHeight(this Component c) => GetRectTransform(c)?.rect.height ?? 0;
    public static float GetHeight(this Transform t) => GetRectTransform(t)?.rect.height ?? 0;

    public static void SetWidth(this RectTransform rt, float value) => rt?.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
    public static void SetWidth(this GameObject go, float value) => GetRectTransform(go)?.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
    public static void SetWidth(this Component c, float value) => GetRectTransform(c)?.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
    public static void SetWidth(this Transform t, float value) => GetRectTransform(t)?.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
    public static void SetHeight(this RectTransform rt, float value) => rt?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
    public static void SetHeight(this GameObject go, float value) => GetRectTransform(go)?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
    public static void SetHeight(this Component c, float value) => GetRectTransform(c)?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
    public static void SetHeight(this Transform t, float value) => GetRectTransform(t)?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);

    public static int GetChildCount(this GameObject go) => go.transform.childCount;
    public static int GetChildCount(this Component c) => c.transform.childCount;

    [Obsolete("Instead use: GetComponentsInImmediateChildren<T>(this GameObject gameObject, bool includeInactive)", true)]
    public static T[] GetComponentsInImmediateChildren<T>(this GameObject gameObject) where T : Component
        => GetComponentsInImmediateChildren<T>(gameObject.transform);
    [Obsolete("Instead use: GetComponentsInImmediateChildren<T>(this Component component, bool includeInactive)", true)]
    public static T[] GetComponentsInImmediateChildren<T>(this Component component) where T : Component
        => GetComponentsInImmediateChildren<T>(component.transform);
    [Obsolete("Instead use: GetComponentsInImmediateChildren<T>(this Transform transform, bool includeInactive)", true)]
    public static T[] GetComponentsInImmediateChildren<T>(this Transform transform) where T : Component
    {
        List<T> list = new List<T>(transform.childCount);
        foreach (Transform child in transform)
            if (child.TryGetComponent<T>(out T t))
                list.Add(t);
        return list.ToArray();
    }

    public static IEnumerable<T> GetComponentsInImmediateChildren<T>(this GameObject gameObject, bool includeInactive) where T : Component
        => GetComponentsInImmediateChildren<T>(gameObject.transform, includeInactive);
    public static IEnumerable<T> GetComponentsInImmediateChildren<T>(this Component component, bool includeInactive) where T : Component
        => GetComponentsInImmediateChildren<T>(component.transform, includeInactive);
    public static IEnumerable<T> GetComponentsInImmediateChildren<T>(this Transform transform, bool includeInactive) where T : Component
    {
        foreach (Transform child in includeInactive ? transform.GetChildren() : transform.GetActiveChildren())
            if (child.TryGetComponent(out T t))
                yield return t;
    }

    /// <summary>
    /// Gets the sibling index of the component by invoking the function on its transform.
    /// </summary>
    /// <returns>The sibling index of the component's transform or -1 if null</returns>
    public static int GetSiblingIndex(this Component component) => component ? component.transform.GetSiblingIndex() : -1;

    /// <summary>
    /// Gets the active sibling index of the component by invoking the function on its transform.
    /// <br>See also: <seealso cref="GetActiveSiblingIndex(Transform)"/></br>
    /// </summary>
    /// <returns>The active sibling index of the component's transform or -1 if null</returns>
    public static int GetActiveSiblingIndex(this Component component) => component ? component.transform.GetActiveSiblingIndex() : -1;

    /// <summary>
    /// Get the ActiveSiblingIndex of the transform by disregarding any inactive sibling objects
    /// </summary>
    /// <returns>The sibling index of the transform or -1 if null</returns>
    public static int GetActiveSiblingIndex(this Transform transform)
    {
        if (!transform) return -1;
        int siblingIndex = transform.GetSiblingIndex();
        int activeSiblingIndex = siblingIndex;
        for (int i = 0; i < siblingIndex; i++)
            if (!transform.parent.GetChild(i).gameObject.activeSelf)
                activeSiblingIndex--;
        return activeSiblingIndex;
    }

    public static int GetActiveChildCount(this Transform transform)
    {
        if (!transform) return default;
        int activeSiblingIndex = 0;
        for (int i = 0; i < transform.childCount; i++)
            if (transform.GetChild(i).gameObject.activeSelf)
                activeSiblingIndex++;
        return activeSiblingIndex;
    }

    public static Transform GetActiveChild(this Transform transform, int activeChildID)
    {
        if (!transform) return default;
        int activeSiblingIndex = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.activeSelf && activeSiblingIndex++ == activeChildID)
                return child;
        }
        return default;
    }

    public static IEnumerable<Transform> GetChildren(this GameObject gameObject) => GetChildren(gameObject.transform);

    public static IEnumerable<Transform> GetChildren(this Component component) => GetChildren(component.transform);

    public static IEnumerable<Transform> GetChildren(this Transform transform)
    {
        for (int i = 0; i < transform.childCount; i++)
            yield return transform.GetChild(i);
    }

    public static IEnumerable<Transform> GetActiveChildren(this Transform transform)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.activeSelf)
                yield return child;
        }
    }

    #endregion

    #region SetActive
    public static void SetActive(this Transform transform, bool value) => transform.gameObject.SetActive(value);
    public static void ToggleActive(this GameObject gameObject) => gameObject.SetActive(!gameObject.activeSelf);
    public static void ToggleActive(this Transform transform) => transform.gameObject.SetActive(!transform.gameObject.activeSelf);
    #endregion

    #region GetParent
    public static GameObject GetParent(this GameObject gameObject) => gameObject.transform.parent.gameObject;
    public static Transform GetParentTransform(this GameObject gameObject) => gameObject.transform.parent;
    public static GameObject GetParentGameObject(this GameObject gameObject) => gameObject.transform.parent.gameObject;

    public static GameObject GetParentWithTag(this GameObject go, string tag)
    {
        if (go == null) return null;

        if (go.CompareTag(tag))
            return go;
        else
            return GetParentWithTag(go.transform.parent, tag)?.gameObject;
    }
    public static Transform GetParentWithTag(this Transform t, string tag)
    {
        if (t == null) return null;

        if (t.CompareTag(tag))
            return t;
        else
            return GetParentWithTag(t.parent, tag);
    }

    public static bool TryGetParentWithTag(this GameObject go, string tag, out GameObject parentWithTag)
    {
        parentWithTag = GetParentWithTag(go, tag);
        return parentWithTag != null;
    }

    public static bool TryGetParentWithTag(this Transform t, string tag, out Transform parentWithTag)
    {
        parentWithTag = GetParentWithTag(t, tag);
        return parentWithTag != null;
    }

    public static bool IsChildOf(this Transform t, string name)
    {
        while (t && t.parent)
            if (t.parent.name == name) return true;
            else t = t.parent;
        return false;
    }
    #endregion

    #region Destroy
    public static void Destroy(this GameObject go) => UnityEngine.Object.Destroy(go);
    public static void Destroy(this GameObject go, float t) => UnityEngine.Object.Destroy(go, t);
    public static void DestroyAllChildren(this Transform transform)
    {
        foreach (Transform t in transform) UnityEngine.Object.Destroy(t.gameObject);
    }
    public static void DestroyAllChildren(this GameObject gameObject)
    {
        foreach (Transform t in gameObject.transform) UnityEngine.Object.Destroy(t.gameObject);
    }
    #endregion

    #region Mathf

    /// <summary> Compares two floating point values and returns true if they are similar. </summary>
    public static bool Approximately(this float f1, float f2)
        => Mathf.Approximately(f1, f2);

    /// <summary> Compares two floating point values and returns true if they are within a defined tolerance. </summary>
    public static bool Approximately(this float f1, float f2, float tolerance)
        => Mathf.Abs(f1 - f2) <= tolerance;

    /// <summary> Compares the vector's components and returns true if they are similar to the parameter. </summary>
    public static bool Approximately(this Vector2 v1, float parameter)
        => Mathf.Approximately(v1.x, parameter) && Mathf.Approximately(v1.y, parameter);

    /// <summary> Compares the vector's components and returns true if they are similar to the parameter within a defined tolerance. </summary>
    public static bool Approximately(this Vector2 v1, float parameter, float tolerance)
        => Approximately(v1.x, parameter, tolerance) && Approximately(v1.y, parameter, tolerance);

    /// <summary> Compares two vectors and returns true if they are similar. </summary>
    public static bool Approximately(this Vector2 v1, Vector2 v2)
        => Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y);

    /// <summary> Compares two vectors and returns true if they are  within a defined tolerance. </summary>
    public static bool Approximately(this Vector2 v1, Vector2 v2, float tolerance)
        => Approximately(v1.x, v2.x, tolerance) && Approximately(v1.y, v2.y, tolerance);

    /// <summary> Compares two vectors and returns true if they are  within a defined tolerance. </summary>
    public static bool Approximately(this Vector2 v1, Vector2 v2, Vector2 tolerance)
        => Approximately(v1.x, v2.x, tolerance.x) && Approximately(v1.y, v2.y, tolerance.y);

    /// <summary>
    /// Returns the absolute value of the vector components.
    /// </summary>
    public static Vector2 Abs(this Vector2 v2)
    {
        v2.x = Mathf.Abs(v2.x);
        v2.y = Mathf.Abs(v2.y);
        return v2;
    }

    /// <summary>
    /// Returns the absolute value of the vector components.
    /// </summary>
    public static Vector3 Abs(this Vector3 v3)
    {
        v3.x = Mathf.Abs(v3.x);
        v3.y = Mathf.Abs(v3.y);
        v3.z = Mathf.Abs(v3.z);
        return v3;
    }

    #endregion

    #region ColliderPositions

    /// <summary>
    /// Gets the world position for this point of the bounds.
    /// </summary>
    public static Vector2 GetTopLeft(this Bounds bounds) => new Vector2(bounds.min.x, bounds.max.y);
    /// <summary>
    /// Gets the world position for this point of the bounds.
    /// </summary>
    public static Vector2 GetTopCenter(this Bounds bounds) => new Vector2(bounds.center.x, bounds.max.y);
    /// <summary>
    /// Gets the world position for this point of the bounds.
    /// </summary>
    public static Vector2 GetTopRight(this Bounds bounds) => new Vector2(bounds.max.x, bounds.max.y);
    /// <summary>
    /// Gets the world position for this point of the bounds.
    /// </summary>
    public static Vector2 GetCenterLeft(this Bounds bounds) => new Vector2(bounds.min.x, bounds.center.y);
    /// <summary>
    /// Gets the world position for this point of the bounds.
    /// </summary>
    public static Vector2 GetCenter(this Bounds bounds) => new Vector2(bounds.center.x, bounds.center.y);
    /// <summary>
    /// Gets the world position for this point of the bounds.
    /// </summary>
    public static Vector2 GetCenterRight(this Bounds bounds) => new Vector2(bounds.max.x, bounds.center.y);
    /// <summary>
    /// Gets the world position for this point of the bounds.
    /// </summary>
    public static Vector2 GetBottomLeft(this Bounds bounds) => new Vector2(bounds.min.x, bounds.min.y);
    /// <summary>
    /// Gets the world position for this point of the bounds.
    /// </summary>
    public static Vector2 GetBottomCenter(this Bounds bounds) => new Vector2(bounds.center.x, bounds.min.y);
    /// <summary>
    /// Gets the world position for this point of the bounds.
    /// </summary>
    public static Vector2 GetBottomRight(this Bounds bounds) => new Vector2(bounds.max.x, bounds.min.y);
    #endregion

    #region Texture

    public static bool IsBlank(this Texture2D texture, bool isRA = false) => IsBlank(texture, 0, 0, texture.width, texture.height, isRA);
    public static bool IsBlank(this Texture2D texture, Rect rect, bool isRA = false) => IsBlank(texture, (int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height, isRA);
    public static bool IsBlank(this Texture2D texture, int x, int y, int width, int height, bool isRA = false)
        => isRA
        ? texture.GetPixels(x, y, width, height).All(color => color.g <= float.Epsilon)
        : texture.GetPixels(x, y, width, height).All(color => color.a <= float.Epsilon);

    #endregion

    #region Find Objects

    /// <summary>
    /// Gets all components that implement an interface on the passed scene
    /// </summary>
    /// <typeparam name="T">The interface to be searched</typeparam>
    /// <param name="scene">The scene where the search should take place</param>
    /// <returns>A list with all component instances of type T on the passed scene</returns>
    public static List<Component> FindObjectsWithInterface(this Scene scene, Type type, bool includeInactive)
    {
        List<Component> results = new List<Component>();
        scene.GetRootGameObjects().ForEach(go => results.AddRange(go.GetComponentsInChildren(type, includeInactive)));
        return results;
    }

    /// <summary>
    /// Gets all components that implement an interface on the passed scene
    /// </summary>
    /// <typeparam name="T">The interface to be searched</typeparam>
    /// <param name="scene">The scene where the search should take place</param>
    /// <returns>A list with all component instances of type T on the passed scene</returns>
    public static List<T> FindObjectsWithInterface<T>(this Scene scene)
    {
        List<T> results = new List<T>();
        scene.GetRootGameObjects().ForEach(go => go.GetComponentsInChildren(results));
        return results;
    }

    #endregion

    #region Others

    public static Toggle AddListener(this Toggle toggle, UnityAction<bool> action)
    {
        toggle.onValueChanged.AddListener(action);
        return toggle;
    }

    /// <summary>
    /// Adds a listener to the toggles that will return their index when triggered
    /// </summary>
    /// <param name="toggles">An array of toggles to listen to</param>
    /// <param name="action">An action to be invoked with the toggle's original index</param>
    /// <returns></returns>
    public static Toggle[] AddListener(this Toggle[] toggles, UnityAction<int, bool> action)
    {
        for (int i = 0; i < toggles.Length; i++)
        {
            int j = i; //Using temporary int because i gets updated as a reference if used directly in a delegate
            toggles[i].onValueChanged.AddListener((bool b) => action.Invoke(j, b));
        }
        return toggles;
    }

    public static string GetFullName(this Component component) => GetFullName(component.gameObject);

    public static string GetFullName(this GameObject gameObject)
    {
        if (gameObject == null) return null;

        StringBuilder sb = new StringBuilder(gameObject.scene.name);
        Stack<Transform> stack = new Stack<Transform>();
        for (Transform t = gameObject.transform; t; t = t.parent)
            stack.Push(t);
        while (stack.Count > 0)
            sb.Append("-").Append(stack.Pop().name);

        return sb.ToString();
    }

    public static void CopyValuesTo<T>(this T original, T destination) where T : Component
    {
        if (original && destination)
        {
            original.GetType().GetFields().ForEach(f => f.SetValue(destination, f.GetValue(original)));
            original.GetType().GetProperties().ForEach(p => { if (p.SetMethod != null) p.SetValue(destination, p.GetValue(original)); });
        }
    }

    /// <summary>
    /// Returns a random int within [range.Min(Inclusive)..range.Max(Exclusive)]
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public static float Random(this IntRange range) => UnityEngine.Random.Range(range.Min, range.Max);

    /// <summary>
    /// Returns a random float within [range.Min(Inclusive)..range.Max(Inclusive)]
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public static float Random(this FloatRange range) => UnityEngine.Random.Range(range.Min, range.Max);

    public static bool Includes(this LayerMask layerMask, int layer) => (layerMask.value & (1 << layer)) != 0;

    public static void AddOptionsFromEnum<T>(this Dropdown dropdown, bool clearPreviousOptions) where T : Enum
    {
        if (clearPreviousOptions) dropdown.ClearOptions();
        dropdown.AddOptions(Enum.GetNames(typeof(T)).ToList());
    }

    public static float CombinedBounciness(this PhysicMaterial materialA, PhysicMaterial materialB)
    {
        PhysicMaterialCombine combine = (PhysicMaterialCombine)Mathf.Max((int)materialA.bounceCombine, (int)materialB.bounceCombine);
        switch (combine)
        {
            case PhysicMaterialCombine.Average: return (materialA.bounciness + materialB.bounciness) / 2f;
            case PhysicMaterialCombine.Minimum: return Mathf.Min(materialA.bounciness, materialB.bounciness);
            case PhysicMaterialCombine.Multiply: return materialA.bounciness * materialB.bounciness;
            case PhysicMaterialCombine.Maximum: return Mathf.Max(materialA.bounciness, materialB.bounciness);
            default: throw new NotSupportedException($"Not Supported PhysicMaterialCombine {combine}");
        }
    }

    #endregion

    #region NewCoroutines //Experimenting with EasySingleton

    /// <summary>
    /// Invokes an action only after the yield instruction is complete
    /// </summary>
    /// <param name="yieldInstruction">The instruction to be awaited before the action is invoked</param>
    /// <param name="action">The action to be invoked once the instruction is complete</param>
    /// <returns>A reference to the coroutine that was created</returns>
    public static EasyCoroutine InvokeDelayed(YieldInstruction yieldInstruction, Action action)
        => new EasyCoroutine(yieldInstruction, action);

    /// <summary>
    /// Invokes an action only after the yield instruction is complete
    /// </summary>
    /// <param name="yieldInstruction">The instruction to be awaited before the action is invoked</param>
    /// <param name="action">The action to be invoked once the instruction is complete</param>
    /// <returns>A reference to the coroutine that was created</returns>
    public static EasyCoroutine InvokeDelayed(CustomYieldInstruction yieldInstruction, Action action)
        => new EasyCoroutine(yieldInstruction, action);

    /// <summary>
    /// Invokes an action only after the yield instruction is complete
    /// </summary>
    /// <param name="action">The action to be invoked once the instruction is complete</param>
    /// <param name="yieldInstruction">The instruction to be awaited before the action is invoked</param>
    /// <returns>A reference to the coroutine that was created</returns>
    public static EasyCoroutine InvokeAfter(this Action action, YieldInstruction yieldInstruction)
        => new EasyCoroutine(yieldInstruction, action);

    /// <summary>
    /// Invokes an action only after the yield instruction is complete
    /// </summary>
    /// <param name="action">The action to be invoked once the instruction is complete</param>
    /// <param name="yieldInstruction">The instruction to be awaited before the action is invoked</param>
    /// <returns>A reference to the coroutine that was created</returns>
    public static EasyCoroutine InvokeAfter(this Action action, CustomYieldInstruction yieldInstruction)
        => new EasyCoroutine(yieldInstruction, action);

    #endregion

    #region Coroutines //If this is successfull enough, we could make use of singletons to remove the need to pass a monobehaviour reference

    public static IEnumerator InvokeDelayed(this MonoBehaviour mb, float time, Action a) =>
                                                                            InvokeDelayed(mb, new WaitForSeconds(time), a);

    /// <summary>
    /// Invokes an action only after the yield instruction is complete
    /// </summary>
    /// <param name="mb">A monobehaviour on which a coroutine will be started</param>
    /// <param name="yieldInstruction">The instruction to be awaited before the action is invoked</param>
    /// <param name="action">The action to be invoked once the instruction is complete</param>
    /// <returns>A reference to the coroutine that was created</returns>
    public static IEnumerator InvokeDelayed(this MonoBehaviour mb, YieldInstruction yieldInstruction, Action action)
    {
        IEnumerator enumerator = action.EnumerateAfter(yieldInstruction);
        mb.StartCoroutine(enumerator);
        return enumerator;
    }

    /// <summary>
    /// Invokes an action only after the yield instruction is complete
    /// </summary>
    /// <param name="mb">A monobehaviour on which a coroutine will be started</param>
    /// <param name="yieldInstruction">The instruction to be awaited before the action is invoked</param>
    /// <param name="action">The action to be invoked once the instruction is complete</param>
    /// <returns>A reference to the coroutine that was created</returns>
    public static IEnumerator InvokeDelayed(this MonoBehaviour mb, CustomYieldInstruction yieldInstruction, Action action)
    {
        IEnumerator enumerator = action.EnumerateAfter(yieldInstruction);
        mb.StartCoroutine(enumerator);
        return enumerator;
    }

    /// <summary>
    /// Invokes an action only after the yield instruction is complete
    /// </summary>
    /// <param name="mb">A monobehaviour on which a coroutine will be started</param>
    /// <param name="yieldInstruction">The instruction to be awaited before the action is invoked</param>
    /// <param name="action">The action to be invoked once the instruction is complete</param>
    /// <returns>A reference to the coroutine that was created</returns>
    public static IEnumerator StartCoroutine(this MonoBehaviour mb, YieldInstruction yieldInstruction, Action action)
    {
        IEnumerator enumerator = action.EnumerateAfter(yieldInstruction);
        mb.StartCoroutine(enumerator);
        return enumerator;
    }

    /// <summary>
    /// Invokes an action only after the yield instruction is complete
    /// </summary>
    /// <param name="mb">A monobehaviour on which a coroutine will be started</param>
    /// <param name="yieldInstruction">The instruction to be awaited before the action is invoked</param>
    /// <param name="action">The action to be invoked once the instruction is complete</param>
    /// <returns>A reference to the coroutine that was created</returns>
    public static IEnumerator StartCoroutine(this MonoBehaviour mb, CustomYieldInstruction yieldInstruction, Action action)
    {
        IEnumerator enumerator = action.EnumerateAfter(yieldInstruction);
        mb.StartCoroutine(enumerator);
        return enumerator;
    }

    /// <summary>
    /// Returns an enumerator that fullfills the YieldInstruction then invokes the Action
    /// </summary>
    /// <param name="action">An action to be invoked after the YieldInstruction is complete</param>
    /// <param name="yieldInstruction">An instruction on what to wait for before invoking the Action</param>
    /// <returns>An IEnumerator that yield returns the YieldInstruction then invokes the Action</returns>
    public static IEnumerator EnumerateAfter(this Action action, YieldInstruction yieldInstruction)
    {
        yield return yieldInstruction;
        action.Invoke();
    }

    /// <summary>
    /// Returns an enumerator that fullfills the CustomYieldInstruction then invokes the Action
    /// </summary>
    /// <param name="action">An action to be invoked after the CustomYieldInstruction is complete</param>
    /// <param name="yieldInstruction">An instruction on what to wait for before invoking the Action</param>
    /// <returns>An IEnumerator that yield returns the CustomYieldInstruction then invokes the Action</returns>
    public static IEnumerator EnumerateAfter(this Action action, CustomYieldInstruction yieldInstruction)
    {
        yield return yieldInstruction;
        action.Invoke();
    }

    #endregion
}
