using System.Collections.Generic;
using UnityEngine;
using CogsTowerDefense.Cogs;

namespace CogsTowerDefense.UI
{
    /// <summary>
    /// Zone de stockage pour les rouages en attente de placement
    /// Les rouages achetés sont placés ici avant d'être drag & drop sur le terrain
    /// </summary>
    public class CogStorageZone : MonoBehaviour
    {
        [Header("Storage Settings")]
        [SerializeField] private int maxStorageSlots = 10;
        [SerializeField] private Transform storageContainer; // Container UI pour les slots
        [SerializeField] private GameObject slotPrefab; // Prefab pour un slot de stockage

        [Header("Slot Layout")]
        [SerializeField] private float slotSpacing = 80f;
        [SerializeField] private Vector2 slotSize = new Vector2(70f, 70f);

        // Liste des rouages stockés
        private List<StoredCog> storedCogs = new List<StoredCog>();

        // Slots visuels
        private List<StoredCogSlot> visualSlots = new List<StoredCogSlot>();

        private void Start()
        {
            if (storageContainer == null)
            {
                storageContainer = transform;
            }

            CreateVisualSlots();
        }

        /// <summary>
        /// Crée les slots visuels pour le stockage
        /// </summary>
        private void CreateVisualSlots()
        {
            for (int i = 0; i < maxStorageSlots; i++)
            {
                GameObject slotObj = new GameObject($"StorageSlot_{i}");
                slotObj.transform.SetParent(storageContainer);

                RectTransform rectTransform = slotObj.AddComponent<RectTransform>();
                rectTransform.sizeDelta = slotSize;
                rectTransform.anchoredPosition = new Vector2(0, -i * slotSpacing);

                StoredCogSlot slot = slotObj.AddComponent<StoredCogSlot>();
                slot.Initialize(i, this);
                visualSlots.Add(slot);
            }

            Debug.Log($"Created {maxStorageSlots} storage slots");
        }

        /// <summary>
        /// Ajoute un rouage au stockage
        /// </summary>
        public bool AddCog(CogData cogData)
        {
            if (storedCogs.Count >= maxStorageSlots)
            {
                Debug.LogWarning("Storage is full!");
                return false;
            }

            // Crée un nouvel objet rouage stocké
            StoredCog storedCog = new StoredCog
            {
                cogData = cogData,
                slotIndex = storedCogs.Count
            };

            storedCogs.Add(storedCog);

            // Met à jour le slot visuel
            if (storedCog.slotIndex < visualSlots.Count)
            {
                visualSlots[storedCog.slotIndex].SetCog(storedCog);
            }

            Debug.Log($"Added {cogData.CogName} to storage (slot {storedCog.slotIndex})");
            return true;
        }

        /// <summary>
        /// Retire un rouage du stockage
        /// </summary>
        public bool RemoveCog(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= storedCogs.Count)
            {
                return false;
            }

            storedCogs.RemoveAt(slotIndex);

            // Réorganise les rouages restants
            RefreshSlots();

            Debug.Log($"Removed cog from slot {slotIndex}");
            return true;
        }

        /// <summary>
        /// Obtient le rouage à un slot donné
        /// </summary>
        public StoredCog GetCogAtSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= storedCogs.Count)
            {
                return null;
            }

            return storedCogs[slotIndex];
        }

        /// <summary>
        /// Rafraîchit l'affichage de tous les slots
        /// </summary>
        private void RefreshSlots()
        {
            // Nettoie tous les slots
            foreach (var slot in visualSlots)
            {
                slot.ClearCog();
            }

            // Réassigne les rouages
            for (int i = 0; i < storedCogs.Count; i++)
            {
                storedCogs[i].slotIndex = i;
                if (i < visualSlots.Count)
                {
                    visualSlots[i].SetCog(storedCogs[i]);
                }
            }
        }

        /// <summary>
        /// Obtient le nombre de rouages stockés
        /// </summary>
        public int GetStoredCogCount()
        {
            return storedCogs.Count;
        }

        /// <summary>
        /// Vérifie si le stockage est plein
        /// </summary>
        public bool IsFull()
        {
            return storedCogs.Count >= maxStorageSlots;
        }
    }

    /// <summary>
    /// Représente un rouage stocké dans la zone de stockage
    /// </summary>
    [System.Serializable]
    public class StoredCog
    {
        public CogData cogData;
        public int slotIndex;
    }
}
