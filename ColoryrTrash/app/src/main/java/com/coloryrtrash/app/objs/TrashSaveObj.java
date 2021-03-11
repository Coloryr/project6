package com.coloryrtrash.app.objs;

public class TrashSaveObj implements Comparable<TrashSaveObj> {
    public String UUID;
    public String Nick;
    public double X;
    public double Y;
    public int Capacity;
    public String Time;
    public boolean Open;
    public ItemState State;
    public String SIM;
    public int Battery;

    @Override
    public int compareTo(TrashSaveObj item) {
        return this.Capacity - item.Capacity;
    }
}
