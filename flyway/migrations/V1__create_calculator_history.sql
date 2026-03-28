CREATE TABLE IF NOT EXISTS calculator_operations (
    id BIGSERIAL PRIMARY KEY,
    operation VARCHAR(32) NOT NULL,
    operand_a INTEGER NOT NULL,
    operand_b INTEGER NULL,
    result_text VARCHAR(64) NULL,
    success BOOLEAN NOT NULL DEFAULT TRUE,
    error_message TEXT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_calculator_operations_operation
        CHECK (operation IN ('Add', 'Subtract', 'Multiply', 'Divide', 'Factorial', 'IsPrime'))
);

CREATE INDEX IF NOT EXISTS ix_calculator_operations_created_at
    ON calculator_operations (created_at DESC);

CREATE INDEX IF NOT EXISTS ix_calculator_operations_operation
    ON calculator_operations (operation, created_at DESC);

CREATE INDEX IF NOT EXISTS ix_calculator_operations_success
    ON calculator_operations (success, created_at DESC);

