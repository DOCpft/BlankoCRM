import { FC, useState } from 'react';
import { Button, Div, Group, Header, NavIdProps, Panel, PanelHeader, Placeholder } from '@vkontakte/vkui';

import { EntityFormModal } from '../components/EntityFormModal';
import { DataGrid } from '../components/DataGrid';

interface Client {
  id: number;
  name: string;
  phone?: string;
  network?: string;
}

export interface ClientsProps extends NavIdProps {}

export const Clients: FC<ClientsProps> = ({ id }) => {
  const [clients, setClients] = useState<Client[]>([]);
  const [isFormOpen, setIsFormOpen] = useState(false);

  const handleAddClient = (values: Record<string, string>) => {
    setClients((currentClients) => [
      ...currentClients,
      {
        id: Date.now(),
        name: values.name.trim(),
        phone: values.phone.trim() || undefined,
      network: values.network.trim() || undefined,
      },
    ]);
    setIsFormOpen(false);
  };

  return (
    <Panel id={id}>
      <PanelHeader>
        <div style={{ width: '100%', textAlign: 'center' }}>Клиенты</div>
      </PanelHeader>
      <Div>
        <Button size="l" mode="primary" onClick={() => setIsFormOpen(true)}>
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
            />
          )}
        </Div>
      </Group>
      <EntityFormModal
        id="add-client"
        title="Новый клиент"
        active={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        onSubmit={handleAddClient}
        fields={[
          { name: 'name', label: 'Имя', placeholder: 'Имя клиента', required: true },
          { name: 'phone', label: 'Телефон', placeholder: '+7 ...', type: 'tel', required: true },
          { name: 'network', label: 'Соц. сеть', placeholder: 'Например, VK или Telegram' },
        ]}
      />
    </Panel>
  );
};

export default Clients;
