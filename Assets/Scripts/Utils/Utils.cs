using UnityEngine;

namespace Utils
{
    public class Vector3Extensions : MonoBehaviour
    {
        /// <summary>
        /// Converts string to vector3 
        /// </summary>
        /// <param name="giver parameter String follow format x,y,z without spaces!"></param>
        /// <returns>
        /// returns vector 3 converted from given
        /// </returns>
        public static Vector3 GetVector3FromString(string String)
        {
            if (String.StartsWith ("(") && String.EndsWith (")")) {
                String = String.Substring(1, String.Length-2);
            }
 
            string[]temp = String.Split(',');
            var floatX = System.Convert.ToSingle(temp[0]);
            var floatY = System.Convert.ToSingle(temp[1]);
            var floatZ = System.Convert.ToSingle(temp[2]);
            var vector3Value = new Vector3(floatX, floatY, floatZ);
            return vector3Value;
        }
        public static Vector3 GetRandomVector(float minRange, float maxRange)
        {
            var vec = new Vector3(Random.Range(minRange,maxRange),Random.Range(minRange,maxRange),Random.Range(minRange,maxRange));
            return vec;
        }

        public static string ConvertVector3ToString(Vector3 vec)
        {
            var str = vec.ToString();
            return str;
        }
    }
    public class Vector2Extensions : MonoBehaviour
    {
        /// <summary>
        /// Converts string to vector2
        /// </summary>
        /// <param name="giver parameter String follow format x,y without spaces!"></param>
        /// <returns>
        /// returns vector 2 converted from given
        /// </returns>
        public static Vector2 GetVector2FromString(string String)
        {
            string[]temp = String.Split(',');
            var floatX = System.Convert.ToSingle(temp[0]);
            var floatY = System.Convert.ToSingle(temp[1]);
            var vector2Value = new Vector3(floatX, floatY);
            return vector2Value;
        }
    }

    public class QuaternionExtension : MonoBehaviour
    {
        public static Quaternion StringToQuaternion(string sQuaternion)
        {
            // Remove the parentheses
            if (sQuaternion.StartsWith("(") && sQuaternion.EndsWith(")"))
            {
                sQuaternion = sQuaternion.Substring(1, sQuaternion.Length - 2);
            }

            // split the items
            string[] sArray = sQuaternion.Split(',');

            // store as a Vector3
            Quaternion result = new Quaternion(
                float.Parse(sArray[0]),
                float.Parse(sArray[1]),
                float.Parse(sArray[2]),
                float.Parse(sArray[3]));

            return result;
        }
    }

    public class Utils : MonoBehaviour
    {
      
    }
}