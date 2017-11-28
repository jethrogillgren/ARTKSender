using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base Water Gameplay Object
public abstract class WGO : BaseGameplayObject
{

    //If true, there are more than 1 input or 1 output.
    public bool multi = false;
    public WGO[] inputs;
    public WGO[] outputs;

    //if not multi, you can use these mono instead (LinkedList still populated)
    public WGO input = null;
    public WGO output = null;

    //Default toggle action.  Not implemented if there are no actions.
    public virtual bool Act() { return false; }
    public bool acting = false;//Track and start state

    //Default Water In Action  Not implemented if it cannot recieve water.
    //return False means the action was invalid (feeding water to a switch)
    //return True means it was valid (feeding water to a Sink or )
    public virtual bool FeedWater(WGO from) { return false; }

    //Default Water Stop Action  Not implemented if it cannot recieve water.
    //return False means the action was invalid (stop feeding water to a switch)
    //return True means it was valid (stop feeding water to a Sink )
    public virtual bool StopWater(WGO from) { return false; }

    //How many water inputs or outputs (negative means more outputs than inputs)
    public int WaterIn = 0;

    //  public abstract void test(); //Must be implemented.  Virtual can be

    //Use these for adding so the LinkedList is included
    public void AddInput(WGO input)
    {
        this.input = input;
        inputs[0] = input;
    }
    public void AddOutput(WGO output)
    {
        this.output = output;
        outputs[0] = output;
    }

    public bool InputIs(System.Type a)
    {
        return input.GetType() == a;
    }
    public bool OutputIs(System.Type a)
    {
        return output.GetType() == a;
    }


    public void test()
    {
        InputIs(typeof(SwitchWGO));
    }
    public void DebugSetColour( Color c ) {
        foreach( Renderer rend in GetComponentsInChildren<Renderer>() ) {
            rend.material.SetColor( c.ToString(), c );
        }
    }
}
