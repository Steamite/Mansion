﻿using Player;
using Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    [UxmlElement("Minimap")]
    public partial class Minimap : VisualElement
    {
        Sprite _map;
        [UxmlAttribute] Sprite mapImage 
        { 
            get => _map; 
            set 
            { 
                _map = value;
                if(childCount > 0)
                {
                    VisualElement element = this.Q<VisualElement>("MapImage");
                    
                    element.style.backgroundImage = new(_map);
                    element.style.minWidth = _map.rect.width;
                    element.style.minHeight = _map.rect.height;
                    // DO NOT TOUCH OR MAP WILL BREAK!!! A magic value for scale
                    scale = 8.6847826086956521739130434782609f;
                }
            } 
        }

        float scale = 0;
        Label locationLabel;


        public Minimap()
        {
            VisualElement map = new();
            map.name = "Minimap";
            Add(map);

            VisualElement element = new();
            element.name = "MapImage";
            PlayerMovement movement;
            if (movement = GameObject.FindFirstObjectByType<PlayerMovement>())
            {
                #region Map Position
                DataBinding binding = BindingUtil.CreateBinding(nameof(PlayerMovement.Position));
                binding.sourceToUiConverters.AddConverter((ref Vector2 pos) =>
                {
                    pos *= scale;
                    return new StyleTranslate(new Translate(pos.x, pos.y));
                });

                element.SetBinding("style.translate", binding);
                element.dataSource = movement;
                #endregion

                #region Room Labeel
                locationLabel = new();
                binding = BindingUtil.CreateBinding(nameof(PlayerMovement.ActiveRoom));
                binding.sourceToUiConverters.AddConverter((ref Room r) => r?.name);

                locationLabel.SetBinding("text", binding);
                locationLabel.dataSource = movement;
                Add(locationLabel);
                #endregion
            }
            map.Add(element);

            #region Visor
            element = new();
            element.name = "VisorContainer";
            element.Add(new());
            element.ElementAt(0).name = "Visor";
            PlayerCamera cam;
            if ((cam = GameObject.FindFirstObjectByType<PlayerCamera>()) != null)
            {
                DataBinding binding = BindingUtil.CreateBinding(nameof(PlayerCamera.yRotation));
                binding.sourceToUiConverters.AddConverter((ref float f) => new StyleRotate(new Rotate(f)));
                element.SetBinding("style.rotate", binding);
                element.dataSource = cam;
            }
            map.Add(element);
            #endregion

            element = new();
            element.name = "Center";
            map.Add(element);            
        }
    }
}
