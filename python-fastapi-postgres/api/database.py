import os
from typing import List, Optional
import psycopg
from psycopg.rows import dict_row
from fastapi import HTTPException

from models import User, UserCreate


class DatabaseManager:
    """Database manager for handling PostgreSQL operations."""
    
    @staticmethod
    async def get_connection():
        """Get database connection using PostgreSQL URI from Aspire."""
        db_uri = os.getenv("DB_URI")
        if not db_uri:
            raise HTTPException(status_code=500, detail="DB_URI not found")
        
        return await psycopg.AsyncConnection.connect(db_uri, row_factory=dict_row)
    
    @staticmethod
    async def initialize_database():
        """Create users table if it doesn't exist."""
        try:
            async with await DatabaseManager.get_connection() as conn:
                async with conn.cursor() as cur:
                    await cur.execute("""
                        CREATE TABLE IF NOT EXISTS users (
                            id SERIAL PRIMARY KEY,
                            name VARCHAR(100) NOT NULL,
                            email VARCHAR(100) UNIQUE NOT NULL,
                            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                        )
                    """)
            print("✓ Database initialized")
        except Exception as e:
            print(f"✗ Database initialization error: {e}")
            raise
    
    @staticmethod
    async def check_health():
        """Check database connection health."""
        try:
            async with await DatabaseManager.get_connection() as conn:
                async with conn.cursor() as cur:
                    await cur.execute("SELECT 1")
            return {"status": "healthy", "database": "connected"}
        except Exception as e:
            raise HTTPException(status_code=500, detail=f"Database error: {str(e)}")


class UserRepository:
    """Repository for user-related database operations."""
    
    @staticmethod
    async def get_all() -> List[User]:
        """Get all users from the database."""
        try:
            async with await DatabaseManager.get_connection() as conn:
                async with conn.cursor() as cur:
                    await cur.execute("SELECT id, name, email FROM users ORDER BY id")
                    rows = await cur.fetchall()
            return [User(**row) for row in rows]
        except Exception as e:
            raise HTTPException(status_code=500, detail=str(e))
    
    @staticmethod
    async def get_by_id(user_id: int) -> User:
        """Get a user by ID."""
        try:
            async with await DatabaseManager.get_connection() as conn:
                async with conn.cursor() as cur:
                    await cur.execute("SELECT id, name, email FROM users WHERE id = %s", (user_id,))
                    row = await cur.fetchone()
            
            if not row:
                raise HTTPException(status_code=404, detail="User not found")
            
            return User(**row)
        except HTTPException:
            raise
        except Exception as e:
            raise HTTPException(status_code=500, detail=str(e))
    
    @staticmethod
    async def create(user: UserCreate) -> User:
        """Create a new user."""
        try:
            async with await DatabaseManager.get_connection() as conn:
                async with conn.cursor() as cur:
                    await cur.execute(
                        "INSERT INTO users (name, email) VALUES (%s, %s) RETURNING id, name, email",
                        (user.name, user.email)
                    )
                    row = await cur.fetchone()
            
            return User(**row)
        except psycopg.errors.UniqueViolation:
            raise HTTPException(status_code=400, detail="Email already exists")
        except Exception as e:
            raise HTTPException(status_code=500, detail=str(e))
    
    @staticmethod
    async def delete(user_id: int) -> dict:
        """Delete a user by ID."""
        try:
            async with await DatabaseManager.get_connection() as conn:
                async with conn.cursor() as cur:
                    await cur.execute("DELETE FROM users WHERE id = %s RETURNING id", (user_id,))
                    row = await cur.fetchone()
            
            if not row:
                raise HTTPException(status_code=404, detail="User not found")
            
            return {"message": f"User {user_id} deleted"}
        except HTTPException:
            raise
        except Exception as e:
            raise HTTPException(status_code=500, detail=str(e))