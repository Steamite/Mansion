using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace Items
{
    public class InteractableItem : MonoBehaviour
    {
        [SerializeField][MinMaxRangeSlider(0.5f, 5)] public Vector2 radiusRange;
    }
}
