namespace EasyUnityInternals
{
    public class EasySingleton : SingletonMB<EasySingleton>
    {
        public static EasySingleton GetInstance => GetOrCreateInstance();
        private void Start() => DontDestroyOnLoad(gameObject);
    }
}
