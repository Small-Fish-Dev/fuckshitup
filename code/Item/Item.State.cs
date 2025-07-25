﻿namespace FUCKSHIT;

public enum ItemState : byte
{
	InContainer,
	Equipped,
	InWorld
}

partial class Item
{
	/// <summary>
	/// The current networked state of this item.
	/// <para>NOTE: Don't touch this if you don't need to.</para>
	/// </summary>
	[Sync]
	public ItemState State
	{
		get => _state;
		set
		{
			_state = value;
			UpdateState();
		}
	}
	private ItemState _state;

	/// <summary>
	/// The container that this item is currently inside of.
	/// </summary>
	[Sync, Change( nameof( OnContainerChanged ) )]
	public Container Container { get; set; }

	/// <summary>
	/// Should this item be for example.. physically simulated?
	/// </summary>
	public bool ShouldSimulate => State is ItemState.InWorld;

	/// <summary>
	/// Should this item be rendered?
	/// </summary>
	public bool ShouldRender => State is ItemState.InWorld or ItemState.Equipped;

	private void UpdateState()
	{
		if ( Renderer.IsValid() )
			Renderer.Enabled = ShouldRender;

		if ( Collider.IsValid() )
			Collider.Enabled = ShouldSimulate;

		if ( Rigidbody.IsValid() )
			Rigidbody.Enabled = ShouldSimulate;

		LocalTransform = global::Transform.Zero;
	}

	public void SetContainer( Container container )
	{
		if ( IsProxy )
			return;

		if ( container == Container ) 
			return;

		Container = container;

		if ( !container.IsValid() )
			return;

		State = ItemState.InContainer;
		SetParentObject( container.GameObject );
	}

	[Rpc.Broadcast]
	public void SetParentObject( GameObject gameObject )
	{
		GameObject.SetParent( !gameObject.IsValid() ? null : gameObject, false );
	}

	private void OnContainerChanged( Container previous, Container value )
	{
		if ( !value.IsValid() ) return;

		var gameObject = value.GameObject;
		GameObject.SetParent( !gameObject.IsValid() ? null : gameObject, true );
	}
}
