using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnfrozenTestWork
{
    public class UnitModelProvider : MonoBehaviour
    {
        [SerializeField]
        private UnitReference[] _units;

        private Dictionary<UnitType, UnitReference> _unitsDic;

        private void Awake()
        {
            _unitsDic = _units.ToDictionary(u => u.UnitType);
        }

        public UnitModel GetUnitPrefab(UnitType unitType)
        {
            if (!_unitsDic.TryGetValue(unitType, out var unitReference))
            {
                throw new System.Exception($"Cannot find prefab for unit type: {unitType}.");
            }
            return unitReference.UnitPrefab;
        }

        public Texture2D GetUnitIconImage(UnitType unitType)
        {
            if (!_unitsDic.TryGetValue(unitType, out var unitReference))
            {
                throw new System.Exception($"Cannot find prefab for unit type: {unitType}.");
            }
            return unitReference.UnitIcon;
        }
    }
}