import { FC } from 'react';
import { Button, Div, Group, Header, NavIdProps, Panel, PanelHeader, Placeholder } from '@vkontakte/vkui';

import { EntityFormModal } from '../components/EntityFormModal';
import { DataGrid } from '../components/DataGrid';
import { useCrmStore } from '../store/useCrmStore';
import { Client } from '../types';
import { useModal } from '../hooks/useModal';

export interface ClientsProps extends NavIdProps {}

export const Clients: FC<ClientsProps> = ({ id }) => {
  const clients = useCrmStore((state) => state.clients);
  const addClient = useCrmStore((state) => state.addClient);
  const updateClient = useCrmStore((state) => state.updateClient);
  const deleteClient = useCrmStore((state) => state.deleteClient);
  const clientModal = useModal<Client>();

  const handleAdd = (values: Record<string, string>) => {
    addClient({
      name: values.name.trim(),
      phone: values.phone.trim() || undefined,
      network: values.network.trim(),
    });
    clientModal.close();
  };

  const handleEdit = (values: Record<string, string>) => {
    if (!clientModal.selected) return;
    updateClient(clientModal.selected.id, {
      name: values.name.trim(),
      phone: values.phone.trim() || undefined,
      network: values.network.trim(),
    });
    clientModal.close();
  };

  const handleDelete = () => {
    if (!clientModal.selected) return;
    deleteClient(clientModal.selected.id);
    clientModal.close();
  };

  return (
    <Panel id={id}>
      <PanelHeader>
        <div style={{ width: '100%', textAlign: 'center' }}>Клиенты</div>
      </PanelHeader>
      <Div>
        <Button size="l" mode="primary" onClick={() => clientModal.open()}>
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
                if (client) clientModal.open(client);
              }}
            />
          )}
        </Div>
      </Group>
      <EntityFormModal
        id="add-client"
        title="Новый клиент"
        active={clientModal.isOpen && !clientModal.selected}
        onClose={clientModal.close}
        onSubmit={handleAdd}
        fields={[
          { name: 'name', label: 'Имя', placeholder: 'Имя клиента', required: true },
          { name: 'network', label: 'Соц. сеть', placeholder: 'Например, VK или Telegram', required: true },
          { name: 'phone', label: 'Телефон', placeholder: '+7 ...', type: 'tel', required: false },
        ]}
      />
      {clientModal.selected && (
        <EntityFormModal
          id="edit-client"
          title="Карточка клиента"
          active={clientModal.isOpen}
          initialValues={{
            name: clientModal.selected.name,
            phone: clientModal.selected.phone || '',
            network: clientModal.selected.network || '',
          }}
          onClose={clientModal.close}
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
