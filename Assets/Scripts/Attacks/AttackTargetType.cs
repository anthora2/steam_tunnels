/*
 * AttackTargetType
 * ----------------
 * This enum defines the different targeting categories that a spell/attack can use.
 * 
 * Each spell in the game chooses ONE of these types, which tells the AttackManager
 * and Targeting systems how the spell should be aimed and applied.
 * 
 * Why this exists:
 * - Provides a clean, shared vocabulary for all spell targeting logic.
 * - Prevents errors caused by string comparisons or magic values.
 * - Makes AttackData more readable and easier to configure.
 * - Allows future systems (UI, targeting reticles, validation) to use a unified type.
 * 
 * Example usage:
 *  - Lightning uses Point targeting (single location strike)
 *  - Healing Circle uses Area targeting (AoE at a location)
 *  - Shield uses Self targeting (applies to caster)
 *  - Revive uses Ally targeting (applies to a teammate)
 * 
 * This file contains ONLY the enum for clean organization.
 */



public enum AttackTargetType
{
    Point, 
    Area, 
    Self,
    Ally, 
    Enemy, 
    Line, 
    Cone
}