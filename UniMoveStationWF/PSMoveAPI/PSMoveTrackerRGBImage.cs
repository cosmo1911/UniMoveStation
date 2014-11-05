/* ----------------------------------------------------------------------------
 * This file was automatically generated by SWIG (http://www.swig.org).
 * Version 2.0.9
 *
 * Do not make changes to this file unless you know what you are doing--modify
 * the SWIG interface file instead.
 * ----------------------------------------------------------------------------- */

namespace io.thp.psmove {

using System;
using System.Runtime.InteropServices;

public class PSMoveTrackerRGBImage : IDisposable {
  public HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal PSMoveTrackerRGBImage(IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new HandleRef(this, cPtr);
  }

  internal static HandleRef getCPtr(PSMoveTrackerRGBImage obj) {
    return (obj == null) ? new HandleRef(null, IntPtr.Zero) : obj.swigCPtr;
  }

  ~PSMoveTrackerRGBImage() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          pinvoke.delete_PSMoveTrackerRGBImage(swigCPtr);
        }
        swigCPtr = new HandleRef(null, IntPtr.Zero);
      }
      GC.SuppressFinalize(this);
    }
  }

  public SWIGTYPE_p_void data {
    set {
      pinvoke.PSMoveTrackerRGBImage_data_set(swigCPtr, SWIGTYPE_p_void.getCPtr(value));
    } 
    get {
      IntPtr cPtr = pinvoke.PSMoveTrackerRGBImage_data_get(swigCPtr);
      SWIGTYPE_p_void ret = (cPtr == IntPtr.Zero) ? null : new SWIGTYPE_p_void(cPtr, false);
      return ret;
    } 
  }

  public int width {
    set {
      pinvoke.PSMoveTrackerRGBImage_width_set(swigCPtr, value);
    } 
    get {
      int ret = pinvoke.PSMoveTrackerRGBImage_width_get(swigCPtr);
      return ret;
    } 
  }

  public int height {
    set {
      pinvoke.PSMoveTrackerRGBImage_height_set(swigCPtr, value);
    } 
    get {
      int ret = pinvoke.PSMoveTrackerRGBImage_height_get(swigCPtr);
      return ret;
    } 
  }

  public int size {
    get {
      int ret = pinvoke.PSMoveTrackerRGBImage_size_get(swigCPtr);
      return ret;
    } 
  }

  public PSMoveTrackerRGBImage() : this(pinvoke.new_PSMoveTrackerRGBImage(), true) {
  }

}

}
