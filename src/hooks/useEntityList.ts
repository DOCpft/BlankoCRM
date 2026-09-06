import { useState } from 'react';

export interface EntityWithId {
  id: string;
}

interface UseEntityListOptions<TEntity extends EntityWithId> {
  initialEntities?: TEntity[];
  createEntity: (values: Record<string, string>, id: string) => TEntity;
  updateEntity: (entity: TEntity, values: Record<string, string>) => TEntity;
}

const createEntityId = (): string => {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }

  return `${Date.now()}-${Math.random().toString(36).slice(2)}`;
};

export const useEntityList = <TEntity extends EntityWithId>({
  initialEntities = [],
  createEntity,
  updateEntity,
}: UseEntityListOptions<TEntity>) => {
  const [entities, setEntities] = useState<TEntity[]>(initialEntities);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [selectedEntity, setSelectedEntity] = useState<TEntity | null>(null);

  const openForm = (entity?: TEntity) => {
    setSelectedEntity(entity ?? null);
    setIsFormOpen(true);
  };

  const closeForm = () => {
    setSelectedEntity(null);
    setIsFormOpen(false);
  };

  const handleAdd = (values: Record<string, string>) => {
    setEntities((currentEntities) => [...currentEntities, createEntity(values, createEntityId())]);
    closeForm();
  };

  const handleEdit = (values: Record<string, string>) => {
    if (!selectedEntity) return;

    setEntities((currentEntities) =>
      currentEntities.map((entity) =>
        entity.id === selectedEntity.id ? updateEntity(entity, values) : entity,
      ),
    );
    closeForm();
  };

  const handleDelete = () => {
    if (!selectedEntity) return;

    setEntities((currentEntities) =>
      currentEntities.filter((entity) => entity.id !== selectedEntity.id),
    );
    closeForm();
  };

  return {
    entities,
    isFormOpen,
    selectedEntity,
    openForm,
    closeForm,
    handleAdd,
    handleEdit,
    handleDelete,
  };
};
