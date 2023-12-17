using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckBuilderManager : Manager<DeckBuilderManager>
{
    [SerializeField] private DeckBuilderController deckBuilderController;

    private void Start() {
        deckBuilderController.SetInitDeck(Player.currentDeck);
    }
}
