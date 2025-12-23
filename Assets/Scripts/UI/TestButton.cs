using UnityEngine;
using UnityEngine.UI;

namespace CogsTowerDefense.UI
{
    /// <summary>
    /// Bouton de test simple pour vérifier que les clics fonctionnent
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class TestButton : MonoBehaviour
    {
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();

            if (button != null)
            {
                button.onClick.AddListener(OnClick);
                Debug.Log("TestButton: Listener ajouté");
            }
        }

        private void OnClick()
        {
            Debug.Log("Hello World!");
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClick);
            }
        }
    }
}
