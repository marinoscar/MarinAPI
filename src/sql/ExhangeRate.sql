-- Drop the table if it already exists to avoid conflicts during re-creation
DROP TABLE IF EXISTS "ExchangeRates";

-- Create the ExchangeRates table
CREATE TABLE "ExchangeRates" (
    "Code" INT NOT NULL,                    -- Unique code for the economic indicator
    "TypeId" INT NOT NULL,                  -- Type identifier (e.g., group or classification ID)
    "Type" TEXT NOT NULL,                   -- Type name (e.g., "Currency", "Rate", etc.)
    "Name" TEXT,                            -- Optional display name of the indicator
    "Value" DOUBLE PRECISION NOT NULL,      -- Numeric value of the indicator
    "Date" TIMESTAMP WITHOUT TIME ZONE NOT NULL, -- Effective date of the indicator

    -- Composite primary key ensures uniqueness by Code, TypeId, and Date
    CONSTRAINT "PK_ExchangeRates" PRIMARY KEY ("Code", "TypeId", "Date")
);

-- Optional: add an index for faster queries by Date
CREATE INDEX "IX_ExchangeRates_Date" ON "ExchangeRates" ("Date");
