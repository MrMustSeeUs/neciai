/*
 * File:    BaseEntity.cs
 * Purpose: Abstract base class providing common audit fields for all
 *          database entities in the NeciAI application.
 *          Demonstrates ENCAPSULATION through protected properties
 *          and INHERITANCE as the parent for all model classes.
 * Author:  Abraham Macias
 * Date:    June 2026
 */

using System;

namespace NeciAI.API.Models
{
    /// <summary>
    /// Abstract base class that all database entities inherit from.
    /// Provides shared audit fields: Id, CreatedAt, UpdatedAt, and IsDeleted.
    /// Using abstract prevents this class from being instantiated directly.
    /// </summary>
    public abstract class BaseEntity
    {
        // Primary key for every entity in the database
        public int Id { get; set; }

        // Automatically records when a record was created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Automatically updates when a record is modified
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Soft delete flag — marks record as deleted without removing from DB
        // This is an industry best practice for financial data auditing
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Called before saving to database to update the timestamp.
        /// This is a virtual method — child classes can OVERRIDE it.
        /// This demonstrates POLYMORPHISM.
        /// </summary>
        public virtual void OnBeforeSave()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Returns a string summary of this entity.
        /// Virtual so each child class can override with specific details.
        /// Demonstrates POLYMORPHISM — same method, different behavior.
        /// </summary>
        public virtual string GetSummary()
        {
            return $"Entity ID: {Id}, Created: {CreatedAt}, Updated: {UpdatedAt}";
        }
    }
}