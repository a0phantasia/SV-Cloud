using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderChooseView : IMonoBehaviour
{
    [SerializeField] private Image leaderImage, emblemImage;
    [SerializeField] private Text craftText;
    [SerializeField] private List<Image> craftBoxImages;
    
    public void Select(int index) {
        CardCraft craft = (CardCraft)index;
        leaderImage?.SetSprite(SpriteResources.GetLeaderProfileSprite(index));
        emblemImage?.SetSprite(SpriteResources.GetCardEmblemSprite(index));
        craftText?.SetText(craft.GetCraftName());

        for (int i = 0; i < craftBoxImages.Count; i++)
        {
            craftBoxImages[i]?.SetColor(i == index ? Color.white : Color.gray);
        }
    }
}
