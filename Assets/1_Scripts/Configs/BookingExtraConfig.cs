using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "BookingExtraConfig", menuName = "Scriptable Objects/BookingExtraConfig")]
public class BookingExtraConfig : ScriptableObject
{
    [System.Serializable]
    public class ExtraConfigData
    {
        public ExtraType type;
        public int maxQuantity;
        public float pricePerUnit;
        public Sprite icon;
    }

    public List<ExtraConfigData> configs = new List<ExtraConfigData>();

    public ExtraConfigData GetConfig(ExtraType type)
    {
        return configs.FirstOrDefault(c => c.type == type);
    }

    public void ValidateConfigs()
    {
        var allTypes = System.Enum.GetValues(typeof(ExtraType)).Cast<ExtraType>();
        foreach (var type in allTypes)
        {
            if (!configs.Any(c => c.type == type))
            {
                configs.Add(new ExtraConfigData { type = type });
            }
        }
    }
}
