using UnityEngine;
using System.Collections;
using Common;

public class MidPoint
{
    public Cell c1, c2;
    public Cell midPoint;
    
    public MidPoint(Cell c1, Cell c2, Cell midPoint)
    {
        this.c1 = c1;
        this.c2 = c2;
        this.midPoint = midPoint;
    }
    
    public bool CompareCells(Cell one, Cell theOther)
    {
        return c1.xPos == one.xPos && c1.yPos == one.yPos && c2.xPos == theOther.xPos && c2.yPos == theOther.yPos;
    }
    
    public string PrintMidPoint()
    {
        string str = null;
        
        str += c1.xPos.ToString() + " " + c1.yPos.ToString() + "\n";
        str += c2.xPos.ToString() + " " + c2.yPos.ToString() + "\n";
        str += midPoint.xPos.ToString() + " " + midPoint.yPos.ToString() + "\n";
        
        return str;
    }
}