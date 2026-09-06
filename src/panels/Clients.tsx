import { FC } from 'react';
import { Button, Div, Group, Header, NavIdProps, Panel, PanelHeader, Placeholder } from '@vkontakte/vkui';

import { EntityFormModal } from '../components/EntityFormModal';
import { DataGrid } from '../components/DataGrid';
import { useEntityList } from '../hooks/useEntityList';

interface Client {
  id: string;
  name: string;
  phone?: string;
  network: string;
}

export interface ClientsProps extends NavIdProps {}

export const Clients: FC<ClientsProps> = ({ id }) => {
  const {
    entities: clients,
    isFormOpen,
    selectedEntity: selectedClient,
    openForm,
    closeForm,
    handleAdd,
    handleEdit,
    handleDelete,
  } = useEntityList<Client>({
    createEntity: (values, entityId) => ({
      id: entityId,
      name: values.name.trim(),
      phone: values.phone.trim() || undefined,
      network: values.network.trim(),
    }),
    updateEntity: (client, values) => ({
      ...client,
      name: values.name.trim(),
      phone: values.phone.trim() || undefined,
      network: values.network.trim(),
    }),
  });

  return (
    <Panel id={id}>
      <PanelHeader>
        <div style={{ width: '100%', textAlign: 'center' }}>Клиенты</div>
      </PanelHeader>
      <Div>
        <Button size="l" mode="primary" onClick={() => openForm()}>
          Добавить клиента
        </Button>
      </Div>
      <Group header={<Header>Список клиентов</Header>}>
        <Div>
          {clients.length === 0 ? (
            <Placeholder>Пусто — сюда будут добавляться клиенты.</Placeholder>
          ) : (
            <DataGrid
              columns={[
                { key: 'name', label: 'Имя' },
                { key: 'phone', label: 'Телефон' },
                { key: 'network', label: 'Соц. сеть' },
              ]}
              rows={clients.map((client) => ({
                id: client.id,
                cells: [client.name, client.phone, client.network],
              }))}
              onRowClick={(clientId) => {
                const client = clients.find((item) => item.id === clientId);
                if (client) openForm(client);
              }}
            />
          )}
        </Div>
      </Group>
      <EntityFormModal
        id="add-client"
        title="Новый клиент"
        active={isFormOpen}
        onClose={closeForm}
        onSubmit={handleAdd}
        fields={[
          { name: 'name', label: 'Имя', placeholder: 'Имя клиента', required: true },
          { name: 'network', label: 'Соц. сеть', placeholder: 'Например, VK или Telegram', required: true },
          { name: 'phone', label: 'Телефон', placeholder: '+7 ...', type: 'tel', required: false },
        ]}
      />
      {selectedClient && (
        <EntityFormModal
          id="edit-client"
          title="Карточка клиента"
          active
          initialValues={{
            name: selectedClient.name,
            phone: selectedClient.phone || '',
            network: selectedClient.network || '',
          }}
          onClose={closeForm}
          onSubmit={handleEdit}
          onDelete={handleDelete}
          fields={[
            { name: 'name', label: 'Имя', placeholder: 'Имя клиента', required: true },
            { name: 'phone', label: 'Телефон', placeholder: '+7 ...', type: 'tel', required: true },
            { name: 'network', label: 'Соц. сеть', placeholder: 'Например, VK или Telegram' },
          ]}
        />
      )}
    </Panel>
  );
};

export default Clients;
