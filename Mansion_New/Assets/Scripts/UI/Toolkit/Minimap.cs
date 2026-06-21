using Player;
using Rooms;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    /// <summary>Visual representation of the players location.</summary>
    [UxmlElement("Minimap")]
    public partial class Minimap : VisualElement
    {
        /// <summary>Map image.</summary>
        Sprite _map;
        /// <inheritdoc cref="_map"/>
        [UxmlAttribute]
        Sprite mapImage
        {
            get => _map;
            set
            {
                _map = value;
                if (childCount > 0 && _map != null)
                {
                    VisualElement element = this.Q<VisualElement>("MapImage");

                    element.style.backgroundImage = new(_map);
                    element.style.minWidth = _map.rect.width;
                    element.style.minHeight = _map.rect.height;
                    ratio = 19.9f;
                }
            }
        }


        Sprite circleSprite;
        [UxmlAttribute]
        Sprite CircleSprite
        {
            get => circleSprite;
            set
            {
                circleSprite = value;
                VisualElement element = this.Q<VisualElement>("Center");
                element.style.backgroundImage = new(circleSprite);

            }
        }

        Sprite visorSprite;
        [UxmlAttribute]
        Sprite VisorSprite
        {
            get => visorSprite;
            set
            {
                visorSprite = value;
                VisualElement element = this.Q<VisualElement>("Visor");
                element.style.backgroundImage = new(visorSprite);
            }
        }




        /// <summary>World to image ration.</summary>
        float ratio;
        /// <summary>Map zoom.</summary>
        float zoom;
        /// <summary>Label with the location name.</summary>
		Label locationLabel;

        /// <summary>
        /// Inits all bindings.
        /// <br/> rotation to <see cref="PlayerCamera.VerticalRot"/>
        /// <br/> locationLabel to <see cref="PlayerMovement.ActiveRoom"/>
        /// <br/> position to <see cref="PlayerMovement.Position"/>
        /// <br/> zoom to <see cref="PlayerMovement.mapZoom"/>
        /// </summary>
		public Minimap()
        {
            PlayerCamera cam = GameObject.FindAnyObjectByType<PlayerCamera>();
            PlayerMovement movement = GameObject.FindAnyObjectByType<PlayerMovement>();
            VisualElement map = new();
            map.name = "Minimap";
            Add(map);

            #region Image
            VisualElement mapImage = new();
            mapImage.name = "MapImage";
            if (movement)
            {
                #region Map Zoom
                DataBinding binding = BindingUtil.CreateBinding(nameof(PlayerMovement.mapZoom));
                binding.sourceToUiConverters.AddConverter((ref float f) =>
                {
                    StyleScale _scale = new StyleScale(new Vector2(f, f));
                    //Debug.Log(_scale.keyword + " " + _scale.value);
                    zoom = f * ratio;
                    return _scale;
                });
                mapImage.SetBinding("style.scale", binding);
                #endregion

                #region Map Position
                binding = BindingUtil.CreateBinding(nameof(PlayerMovement.Position));
                binding.sourceToUiConverters.AddConverter((ref Vector2 pos) =>
                {
                    pos *= zoom;
                    //Debug.Log(pos);
                    return new StyleTranslate(new Translate(pos.x, pos.y));
                });

                mapImage.SetBinding("style.translate", binding);
                mapImage.dataSource = movement;
                #endregion

                #region Room Label
                locationLabel = new();
                binding = BindingUtil.CreateBinding(nameof(PlayerMovement.ActiveRoom));
                binding.sourceToUiConverters.AddConverter((ref Room r) => r?.name);

                locationLabel.SetBinding("text", binding);
                locationLabel.dataSource = movement;
                Add(locationLabel);

                Label keyBindLabel = new(
                    "[ ] - zoom");
                Add(keyBindLabel);
                #endregion
            }
            map.Add(mapImage);
            #endregion

            #region Visor
            VisualElement visorContainer = new();
            visorContainer.name = "VisorContainer";
            visorContainer.style.position = Position.Absolute;
            visorContainer.style.width = Length.Percent(100);
            visorContainer.style.height = Length.Percent(100);

            VisualElement visor = new();
            visor.name = "Visor";
            visor.style.backgroundImage = new(visorSprite);
            visorContainer.Add(visor);

            if (cam)
            {
                DataBinding binding = BindingUtil.CreateBinding(nameof(PlayerCamera.horizontalRot));
                binding.sourceToUiConverters.AddConverter((ref float f) => new StyleRotate(new Rotate(f)));
                visorContainer.SetBinding("style.rotate", binding);
                visorContainer.dataSource = cam;
            }
            map.Add(visorContainer);
            visorContainer.BringToFront();
            #endregion

            #region Center Dot
            VisualElement center = new();
            center.name = "Center";
            center.style.backgroundImage = new(circleSprite);

            center.style.position = Position.Absolute;
            map.Add(center);
            center.BringToFront();
            #endregion
        }
    }
}
