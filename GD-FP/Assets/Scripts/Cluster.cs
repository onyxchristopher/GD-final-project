﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cluster
{
    // how deep in the hierarchy the cluster is
    private int level;

    // the extent of the cluster
    private Rect bounds;

    // the position of the core
    private Vector2 corePosition;

    // the parent core
    private Cluster parentCore;

    // unique id among children of the same parent
    private int id;

    // whether the cluster is complete
    private bool complete;

    public Cluster(int lvl, Rect rect, Vector2 corePos, Cluster parent) {
        level = lvl;
        bounds = rect;
        corePosition = corePos;
        parentCore = parent;
        id = -1;
        complete = false;
    }

    public int getLevel() {
        return level;
    }

    public Rect getBounds() {
        return bounds;
    }

    public Vector2 getCorePosition() {
        return corePosition;
    }

    public Cluster getParentCore() {
        return parentCore;
    }

    public int getId() {
        return id;
    }

    public void setId(int newId) {
        id = newId;
    }

    public bool getComplete() {
        return complete;
    }

    public void setComplete(bool completion) {
        complete = completion;
    }
}
