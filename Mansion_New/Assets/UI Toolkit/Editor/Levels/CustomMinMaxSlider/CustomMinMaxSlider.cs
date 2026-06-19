using Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[UxmlElement]
public partial class CustomMinMaxSlider : VisualElement
{
    FloatField minField;
    MinMaxSlider radiusSlider;
    FloatField maxField;
    public CustomMinMaxSlider() { }
    public CustomMinMaxSlider(string name = "")
    {
        
        Add(new Label(name));
        VisualElement element = new() { style = { flexDirection = FlexDirection.Row} };
        Add(element);
        element.Add(minField = new() 
        { 
            style = 
            {
                minWidth = new Length(10, LengthUnit.Percent), 
                maxWidth = new Length(10, LengthUnit.Percent)
            }
        });
        element.Add(radiusSlider = new() { style = {flexGrow = 1}});
        element.Add(maxField = new()
        {
            style =
            {
                minWidth = new Length(10, LengthUnit.Percent),
                maxWidth = new Length(10, LengthUnit.Percent)
            }
        });

        radiusSlider.lowLimit = 0f;
        radiusSlider.highLimit = 5f;
    }

    public void Bind(SerializedProperty prop)
    {
        radiusSlider.Unbind();
        radiusSlider.BindProperty(prop);

        minField.Unbind();
        minField.BindProperty(prop.FindPropertyRelative("x"));

        maxField.Unbind();
        maxField.BindProperty(prop.FindPropertyRelative("y"));
    }
}
