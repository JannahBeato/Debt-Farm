using System.Collections.Generic;
using UnityEngine;

public class JournalManager : MonoBehaviour
{
    public static JournalManager Instance { get; private set; }

    private readonly List<JournalEntrySaveData> _entries = new();
    public IReadOnlyList<JournalEntrySaveData> Entries => _entries;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void AddOrReplaceEntry(JournalEntrySaveData entry)
    {
        int idx = _entries.FindIndex(e => e.id == entry.id);
        if (idx >= 0) _entries[idx] = entry;
        else _entries.Add(entry);
    }

    public List<JournalEntrySaveData> Export()
    {
        return new List<JournalEntrySaveData>(_entries);
    }

    public void Import(List<JournalEntrySaveData> entries)
    {
        _entries.Clear();
        if (entries != null) _entries.AddRange(entries);
    }
}