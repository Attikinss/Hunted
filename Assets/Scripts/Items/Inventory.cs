using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class Inventory {

    /// <summary>The crew member that this inventory was assigned to.</summary>
    public CrewMember Owner { get => m_Owner; }
    private CrewMember m_Owner;

    /// <summary>The collection of items that make up the inventory.</summary>
    private List<IUsable> m_Items;

    /// <summary>The item referred to when requesting next or previous item.</summary>
    private IUsable m_CurrentItem;

    /// <summary>The maximum size of the inventory.</summary>
    private const int INVENTORY_SIZE = 5;

    public Inventory(CrewMember owner) {
        m_Owner = owner;
        m_Items = new List<IUsable>(INVENTORY_SIZE);
    }

    /// <summary>Added a usable item to the inventory if it isn't already full.</summary>
    /// <param name="item">The item to be added.</param>
    /// <returns>True if item was successfully added.</returns>
    public bool AddItem(IUsable item) {
        if (Full()) {
            // Inventory full, do not add
            return false;
        }
        
        m_Items.Add(item);
        return true;
    }

    /// <summary>Removes a usable item from the inventory if it isn't already empty.</summary>
    /// <param name="item">The item to be removed.</param>
    /// <returns>True if item was successfully removed.</returns>
    public bool RemoveItem(IUsable item) {
        if (Empty()) {
            // Inventory empty, do not remove
            return false;
        }

        // If this assert is triggered, either the user has passed in
        // an arbitrary item or something has gone horribly wrong...
        Debug.Assert(m_Items.Contains(item), "Item not in this inventory!");

        // Clear current item if it's being removed
        if (item == m_CurrentItem) {
            m_CurrentItem = null;
        }

        m_Items.Remove(item);
        return true;
    }

    /// <summary>Retrieves the current item.</summary>
    /// <returns>Null if the item hasn't been assigned or the inventory is empty.</returns>
    public IUsable CurrentItem() {
        return m_CurrentItem;
    }

    /// <summary>Retrieves the next item relative to the current item.</summary>
    /// <remarks>NOTE: If the current item is the last, the next item will be the first.</remarks>
    /// <returns>Next item in inventory rotation.</returns>
    public IUsable NextItem() {
        if (m_CurrentItem == null && !Empty()) {
            m_CurrentItem = m_Items.First();
        }
        else {
            // Get pos of current item
            int index = m_Items.IndexOf(m_CurrentItem);

            // Get next item using modulo wrap around calculation
            m_CurrentItem = m_Items[ModuloWrap(index + 1, m_Items.Count)];
        }

        return m_CurrentItem;
    }

    /// <summary>Retrieves the previous item relative to the current item.</summary>
    /// <remarks>NOTE: If the current item is the first, the next item will be the last.</remarks>
    /// <returns>Previous item in inventory rotation.</returns>
    public IUsable PrevItem() {
        if (m_CurrentItem == null && !Empty()) {
            m_CurrentItem = m_Items.Last();
        }
        else {
            // Get pos of current item
            int index = m_Items.IndexOf(m_CurrentItem);

            // Get prev item using modulo wrap around calculation
            m_CurrentItem = m_Items[ModuloWrap(index - 1, m_Items.Count)];
        }

        return m_CurrentItem;
    }

    /// <summary>Checks if the inventory is empty.</summary>
    /// <returns>True if inventory is empty.</returns>
    public bool Empty() {
        return m_Items.Count == 0;
    }

    /// <summary>Checks if the inventory is full.</summary>
    /// <returns>True if inventory is full.</returns>
    public bool Full() {
        return m_Items.Count == INVENTORY_SIZE;
    }

    // TODO: Move this function into a Utils class
    private int ModuloWrap(int index, int max) {
        return (max + index % max) % max;
    }
}